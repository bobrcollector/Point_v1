using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class MyEventsViewModel : BaseViewModel
{
    private readonly IAuthStateService _authStateService; // ИЗМЕНИЛ ТИП
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    public MyEventsViewModel(IAuthStateService authStateService, IDataService dataService, INavigationService navigationService) // ИЗМЕНИЛ ПАРАМЕТР
    {
        _authStateService = authStateService; // ИЗМЕНИЛ
        _dataService = dataService;
        _navigationService = navigationService;

        // Команды
        SwitchTabCommand = new Command<string>(async (tab) => await SwitchTab(tab));
        LoadEventsCommand = new Command(async () => await LoadEvents());
        EditEventCommand = new Command<string>(async (eventId) => await EditEvent(eventId));
        LeaveEventCommand = new Command<string>(async (eventId) => await LeaveEvent(eventId));

        // Инициализация
        SelectedTab = "Created";
        _ = LoadEvents();
    }


    private string _selectedTab;
    public string SelectedTab
    {
        get => _selectedTab;
        set => SetProperty(ref _selectedTab, value);
    }

    private List<Event> _currentEvents = new();
    public List<Event> CurrentEvents
    {
        get => _currentEvents;
        set => SetProperty(ref _currentEvents, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _emptyViewMessage = "Загрузка событий...";
    public string EmptyViewMessage
    {
        get => _emptyViewMessage;
        set => SetProperty(ref _emptyViewMessage, value);
    }

    // Команды
    public ICommand SwitchTabCommand { get; }
    public ICommand LoadEventsCommand { get; }
    public ICommand EditEventCommand { get; }
    public ICommand LeaveEventCommand { get; }

    public bool ShowEditButton => SelectedTab == "Created";
    public bool ShowLeaveButton => SelectedTab == "Participating";

    private async Task SwitchTab(string tab)
    {
        SelectedTab = tab;
        OnPropertyChanged(nameof(ShowEditButton));
        OnPropertyChanged(nameof(ShowLeaveButton));
        await LoadEvents();
    }

    public async Task LoadEvents()
    {
        if (!_authStateService.IsAuthenticated)
        {
            EmptyViewMessage = "Войдите, чтобы увидеть свои события";
            CurrentEvents = new List<Event>();
            System.Diagnostics.Debug.WriteLine("🔐 Пользователь не авторизован");
            return;
        }

        try
        {
            IsLoading = true;
            var userId = _authStateService.CurrentUserId;
            System.Diagnostics.Debug.WriteLine($"🔄 Загрузка событий для вкладки: {SelectedTab}, пользователь: {userId}");

            List<Event> events = new();

            switch (SelectedTab)
            {
                case "Created":
                    events = await _dataService.GetUserEventsAsync(userId);
                    EmptyViewMessage = "Вы еще не создали ни одного события";
                    System.Diagnostics.Debug.WriteLine($"📥 Запрошены созданные события, получено: {events.Count}");
                    break;

                case "Participating":
                    events = await _dataService.GetParticipatingEventsAsync(userId);
                    EmptyViewMessage = "Вы еще не участвуете ни в одном событии";
                    System.Diagnostics.Debug.WriteLine($"📥 Запрошены события участия, получено: {events.Count}");
                    break;

                case "Archived":
                    events = await _dataService.GetArchivedEventsAsync(userId);
                    EmptyViewMessage = "У вас нет завершенных событий";
                    System.Diagnostics.Debug.WriteLine($"📥 Запрошены архивные события, получено: {events.Count}");
                    break;
            }

            CurrentEvents = events;
            System.Diagnostics.Debug.WriteLine($"✅ Загружено событий: {events.Count} для вкладки {SelectedTab}");

            // Отладочная информация о каждом событии
            foreach (var eventItem in events)
            {
                System.Diagnostics.Debug.WriteLine($"📄 Событие: {eventItem.Title}, Creator: {eventItem.CreatorId}, Participants: {eventItem.ParticipantIds?.Count ?? 0}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки событий: {ex.Message}");
            EmptyViewMessage = "Ошибка загрузки событий";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task EditEvent(string eventId)
    {
        try
        {
            // Устанавливаем ID события и переходим к деталям
            GlobalEventId.EventId = eventId;
            await Shell.Current.GoToAsync("//EventDetailsPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка перехода к редактированию: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось открыть событие для редактирования", "OK");
        }
    }
    private async Task LeaveEvent(string eventId)
    {
        try
        {
            var success = await _dataService.LeaveEventAsync(eventId, _authStateService.CurrentUserId); // ИЗМЕНИЛ
            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Успех", "Вы вышли из события", "OK");
                await LoadEvents();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось выйти из события", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}