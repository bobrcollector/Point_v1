using Point_v1.Models;
using Point_v1.Services;
using Point_v1.Views;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class ReportsManagementViewModel : BaseViewModel
{
    private readonly IReportService _reportService;
    private readonly IDataService _dataService;
    private readonly IAuthorizationService _authService;
    private readonly IAuthStateService _authStateService;
    private readonly NavigationStateService _navigationService;

    public ReportsManagementViewModel(
        IReportService reportService,
        IDataService dataService,
        IAuthorizationService authService,
        IAuthStateService authStateService,
        NavigationStateService navigationService) 
    {
        _reportService = reportService;
        _dataService = dataService;
        _authService = authService;
        _authStateService = authStateService;
        _navigationService = navigationService;

        LoadReportsCommand = new Command(async () => await LoadReports());
        SwitchTabCommand = new Command<string>(async (tab) => await SwitchTab(tab));
        ResolveReportCommand = new Command<Report>(async (report) => await ResolveReport(report));
        ViewEventCommand = new Command<string>(async (eventId) => await ViewEvent(eventId));
        BlockEventCommand = new Command<string>(async (eventId) => await BlockEvent(eventId));
        GoBackCommand = new Command(async () => await GoBack());

        SelectedTab = "Pending";
        _ = LoadReports();
    }
    
    public ICommand GoBackCommand { get; }

    private string _selectedTab;
    public string SelectedTab
    {
        get => _selectedTab;
        set => SetProperty(ref _selectedTab, value);
    }

    private List<Report> _reports = new();
    public List<Report> Reports
    {
        get => _reports;
        set => SetProperty(ref _reports, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _emptyMessage = "Загрузка жалоб...";
    public string EmptyMessage
    {
        get => _emptyMessage;
        set => SetProperty(ref _emptyMessage, value);
    }

    public ICommand LoadReportsCommand { get; }
    public ICommand SwitchTabCommand { get; }
    public ICommand ResolveReportCommand { get; }
    public ICommand ViewEventCommand { get; }
    public ICommand BlockEventCommand { get; }

    public async Task LoadReports()
    {
        try
        {
            if (_reportService == null || _authService == null || _authStateService == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ Сервисы не инициализированы");
                EmptyMessage = "Ошибка инициализации";
                return;
            }

            if (!_authStateService.IsAuthenticated || !await _authService.IsModeratorAsync())
            {
                System.Diagnostics.Debug.WriteLine("❌ Нет прав для просмотра жалоб");
                EmptyMessage = "Недостаточно прав";
                return;
            }

            IsLoading = true;
            EmptyMessage = "Загрузка...";

            System.Diagnostics.Debug.WriteLine($"🔄 Загрузка жалоб для вкладки: {SelectedTab}");

            switch (SelectedTab)
            {
                case "Pending":
                    Reports = await _reportService.GetPendingReportsAsync();
                    EmptyMessage = "Нет ожидающих жалоб";
                    break;
                case "Resolved":
                    Reports = await _reportService.GetResolvedReportsAsync();
                    EmptyMessage = "Нет решенных жалоб";
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"✅ Загружено жалоб: {Reports.Count} для вкладки {SelectedTab}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки жалоб: {ex.Message}");
            EmptyMessage = "Ошибка загрузки жалоб";
            Reports = new List<Report>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SwitchTab(string tab)
    {
        SelectedTab = tab;
        await LoadReports();
    }

    private async Task ResolveReport(Report report)
    {
        if (report == null) return;

        var action = await Application.Current.MainPage.DisplayActionSheet(
            $"Жалоба #{report.Id.Substring(0, 8)}",
            "Отмена",
            null,
            "✅ Одобрить",
            "❌ Отклонить",
            "🚫 Заблокировать событие",
            "👁️ Просмотр события"
        );

        switch (action)
        {
            case "✅ Одобрить":
                await ResolveWithNotes(report, true);
                break;
            case "❌ Отклонить":
                await ResolveWithNotes(report, false);
                break;
            case "🚫 Заблокировать событие":
                await BlockEvent(report.TargetEventId);
                break;
            case "👁️ Просмотр события":
                await ViewEvent(report.TargetEventId);
                break;
        }
    }

    private async Task ResolveWithNotes(Report report, bool approve)
    {
        var notes = await Application.Current.MainPage.DisplayPromptAsync(
            "Комментарий модератора",
            approve ? "Укажите причину одобрения:" : "Укажите причину отклонения:",
            "Подтвердить",
            "Отмена",
            maxLength: 500
        );

        if (string.IsNullOrEmpty(notes))
        {
            return;
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
            
            await LoadReports();
        }
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
                return;
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
                await LoadReports();
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

    public async Task GoBack()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔙 Выполняется возврат на предыдущую страницу...");
            
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                await Shell.Current.GoToAsync("..");
                System.Diagnostics.Debug.WriteLine("✅ Возврат на предыдущую страницу через ..");
            }
            else
            {
                await Shell.Current.GoToAsync("///ModeratorDashboard");
                System.Diagnostics.Debug.WriteLine("✅ Возврат на админ-панель");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка возврата: {ex.Message}");
            try
            {
                await Shell.Current.GoToAsync("///ModeratorDashboard");
                System.Diagnostics.Debug.WriteLine("✅ Fallback: возврат на админ-панель");
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка fallback возврата: {fallbackEx.Message}");
            }
        }
    }
}