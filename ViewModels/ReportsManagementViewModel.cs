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

        SelectedTab = "Pending";
        _ = LoadReports();
    }

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

    public async Task LoadReports()
    {
        try
        {
            // ДОБАВЬТЕ ПРОВЕРКУ СЕРВИСОВ
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
                await LoadReports();
            }
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

            // Проверяем, существует ли событие
            var eventItem = await _dataService.GetEventAsync(eventId);
            if (eventItem == null)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Событие не найдено или было удалено", "OK");
                return;
            }

            // ПЕРЕДАЕМ ПАРАМЕТР, ЧТО ПРИШЛИ ИЗ ЖАЛОБ
            await Shell.Current.GoToAsync($"{nameof(EventDetailsPage)}?eventId={eventId}&fromReports=true");
            System.Diagnostics.Debug.WriteLine($"✅ Переход к событию выполнен (из жалоб)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка перехода к событию: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось открыть событие", "OK");
        }
    }
}