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

    private async Task ViewAllReports()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 Команда перехода к жалобам...");
            await Shell.Current.GoToAsync("///ReportsManagementPage");
            System.Diagnostics.Debug.WriteLine("✅ Успешный переход к ReportsManagementPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка перехода: {ex.Message}");
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

        if (!string.IsNullOrEmpty(notes))
        {
            var success = await _reportService.ResolveReportAsync(
                report.Id,
                _authStateService.CurrentUserId,
                approve,
                notes
            );

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Успех",
                    approve ? "Жалоба одобрена" : "Жалоба отклонена",
                    "OK"
                );
                await RefreshData();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось обработать жалобу", "OK");
            }
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
}