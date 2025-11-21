using Point_v1.Models;
using Point_v1.Services;
using Point_v1.Views;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class ModeratorDashboardViewModel : BaseViewModel
{
    private readonly IAuthorizationService _authService;
    private readonly IReportService _reportService;
    private readonly IDataService _dataService;
    private readonly IAuthStateService _authStateService;

    public ModeratorDashboardViewModel(
        IAuthorizationService authService,
        IReportService reportService,
        IDataService dataService,
        IAuthStateService authStateService)
    {
        _authService = authService;
        _reportService = reportService;
        _dataService = dataService;
        _authStateService = authStateService;

        LoadDashboardCommand = new Command(async () => await LoadDashboardData());
        RefreshCommand = new Command(async () => await RefreshData());
        ResolveReportCommand = new Command<Report>(async (report) => await ResolveReport(report));
        ViewAllReportsCommand = new Command(async () => await ViewAllReports());
        ViewEventCommand = new Command<string>(async (eventId) => await ViewEvent(eventId));

        _ = LoadDashboardData();
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private int _pendingReportsCount;
    public int PendingReportsCount
    {
        get => _pendingReportsCount;
        set => SetProperty(ref _pendingReportsCount, value);
    }

    private List<Report> _recentReports = new();
    public List<Report> RecentReports
    {
        get => _recentReports;
        set => SetProperty(ref _recentReports, value);
    }

    private bool _isModerator;
    public bool IsModerator
    {
        get => _isModerator;
        set => SetProperty(ref _isModerator, value);
    }

    private int _resolvedReportsCount;
    public int ResolvedReportsCount
    {
        get => _resolvedReportsCount;
        set => SetProperty(ref _resolvedReportsCount, value);
    }

    public ICommand LoadDashboardCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ResolveReportCommand { get; }
    public ICommand ViewAllReportsCommand { get; }
    public ICommand ViewEventCommand { get; }

    private async Task ViewAllReports()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 Команда перехода к жалобам...");
            await Shell.Current.GoToAsync(nameof(ReportsManagementPage));
            System.Diagnostics.Debug.WriteLine("✅ Успешный переход к ReportsManagementPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка перехода: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось открыть страницу жалоб", "OK");
        }
    }
    public async Task LoadDashboardData()
    {
        if (!_authStateService.IsAuthenticated)
            return;

        try
        {
            IsLoading = true;

            IsModerator = await _authService.IsModeratorAsync();
            if (!IsModerator)
            {
                System.Diagnostics.Debug.WriteLine("❌ Пользователь не имеет прав модератора");
                return;
            }

            var pendingReportsTask = _reportService.GetPendingReportsCountAsync();
            var recentReportsTask = _reportService.GetPendingReportsAsync();
            var resolvedReportsTask = _reportService.GetResolvedReportsAsync();

            await Task.WhenAll(pendingReportsTask, recentReportsTask, resolvedReportsTask);

            PendingReportsCount = pendingReportsTask.Result;
            RecentReports = recentReportsTask.Result.Take(5).ToList();
            ResolvedReportsCount = resolvedReportsTask.Result.Count;

            System.Diagnostics.Debug.WriteLine($"📊 Дашборд загружен: {PendingReportsCount} ожидает, {ResolvedReportsCount} решено, {RecentReports.Count} недавних");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки дашборда: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    public async Task RefreshData()
    {
        await LoadDashboardData();
    }

    private async Task ResolveReport(Report report)
    {
        if (report == null) return;

        try
        {
            var action = await Application.Current.MainPage.DisplayActionSheet(
                $"Жалоба на событие {report.TargetEventId}",
                "Отмена",
                null,
                "✅ Одобрить жалобу",
                "❌ Отклонить жалобу",
                "🚫 Заблокировать событие",
                "📋 Подробности"
            );

            switch (action)
            {
                case "✅ Одобрить жалобу":
                    await ResolveReportWithNotes(report, true);
                    break;
                case "❌ Отклонить жалобу":
                    await ResolveReportWithNotes(report, false);
                    break;
                case "🚫 Заблокировать событие":
                    await BlockEvent(report.TargetEventId);
                    break;
                case "📋 Подробности":
                    await ShowReportDetails(report);
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка решения жалобы: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось обработать жалобу", "OK");
        }
    }

    private async Task ResolveReportWithNotes(Report report, bool approve)
    {
        var notes = await Application.Current.MainPage.DisplayPromptAsync(
            "Комментарий модератора",
            approve ? "Укажите причину одобрения жалобы:" : "Укажите причину отклонения жалобы:",
            "Подтвердить",
            "Отмена",
            maxLength: 500
        );

        if (string.IsNullOrEmpty(notes))
        {
            return; // Пользователь отменил
        }

        bool shouldBlock = false;
        if (approve)
        {
            shouldBlock = await Application.Current.MainPage.DisplayAlert(
                "Блокировка события",
                "Хотите заблокировать это событие?",
                "Да, заблокировать",
                "Нет, только одобрить жалобу"
            );
        }

        var success = await _reportService.ResolveReportAsync(
            report.Id,
            _authStateService.CurrentUserId,
            approve,
            notes
        );

        if (success)
        {
            if (shouldBlock)
            {
                var blockSuccess = await _dataService.BlockEventAsync(
                    report.TargetEventId,
                    _authStateService.CurrentUserId,
                    notes
                );

                if (blockSuccess)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Успех",
                        "Жалоба одобрена и событие заблокировано",
                        "OK"
                    );
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Частичный успех",
                        "Жалоба одобрена, но не удалось заблокировать событие",
                        "OK"
                    );
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Успех",
                    approve ? "Жалоба одобрена" : "Жалоба отклонена",
                    "OK"
                );
            }
            
            await RefreshData();
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось обработать жалобу", "OK");
        }
    }

    private async Task ShowReportDetails(Report report)
    {
        var eventItem = await _dataService.GetEventAsync(report.TargetEventId);
        var eventTitle = eventItem?.Title ?? "Неизвестное событие";

        var message = $"""
            📅 Событие: {eventTitle}
            🚨 Тип жалобы: {GetReportTypeText(report.Type)}
            📝 Причина: {report.Reason}
            ⏰ Дата: {report.CreatedAt:dd.MM.yyyy HH:mm}
            """;

        await Application.Current.MainPage.DisplayAlert("Детали жалобы", message, "OK");
    }

    private string GetReportTypeText(ReportType type)
    {
        return type switch
        {
            ReportType.Spam => "Спам",
            ReportType.Inappropriate => "Неуместный контент",
            ReportType.Scam => "Мошенничество",
            ReportType.Illegal => "Нелегальная деятельность",
            ReportType.Other => "Другое",
            _ => "Неизвестно"
        };
    }

    private async Task ViewEvent(string eventId)
    {
        try
        {
            if (string.IsNullOrEmpty(eventId))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "ID события не найден", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"🔄 Переход к событию: {eventId}");

            var eventItem = await _dataService.GetEventAsync(eventId);
            if (eventItem == null)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Событие не найдено или было удалено", "OK");
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(EventDetailsPage)}?eventId={eventId}&fromReports=true");
            System.Diagnostics.Debug.WriteLine($"✅ Переход к событию выполнен (из жалоб)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка перехода к событию: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось открыть событие", "OK");
        }
    }

    private async Task BlockEvent(string eventId)
    {
        try
        {
            if (string.IsNullOrEmpty(eventId))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "ID события не найден", "OK");
                return;
            }

            var reason = await Application.Current.MainPage.DisplayPromptAsync(
                "Блокировка события",
                "Укажите причину блокировки события:",
                "Заблокировать",
                "Отмена",
                maxLength: 500,
                keyboard: Keyboard.Default
            );

            if (string.IsNullOrWhiteSpace(reason))
            {
                return; // Пользователь отменил
            }

            var confirm = await Application.Current.MainPage.DisplayAlert(
                "Подтверждение",
                "Вы уверены, что хотите заблокировать это событие?",
                "Да, заблокировать",
                "Отмена"
            );

            if (!confirm)
            {
                return;
            }

            var success = await _dataService.BlockEventAsync(
                eventId,
                _authStateService.CurrentUserId,
                reason
            );

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Успех",
                    "Событие успешно заблокировано",
                    "OK"
                );
                await LoadDashboardData();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Не удалось заблокировать событие",
                    "OK"
                );
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка блокировки события: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось заблокировать событие", "OK");
        }
    }
}