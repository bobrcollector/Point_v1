using Point_v1.Services; // ДОБАВЬ ЭТУ СТРОЧКУ В НАЧАЛЕ ФАЙЛА
using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly IAuthStateService _authStateService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    public HomeViewModel(IAuthStateService authStateService, IDataService dataService, INavigationService navigationService)
    {
        _authStateService = authStateService;
        _dataService = dataService;
        _navigationService = navigationService;

        _authStateService.AuthenticationStateChanged += OnAuthenticationStateChanged;

        GoToLoginCommand = new Command(async () => await GoToLogin());
        CreateEventCommand = new Command(async () => await CreateEvent());
        JoinEventCommand = new Command<string>(async (eventId) => await JoinEvent(eventId));
        OpenFiltersCommand = new Command(async () => await OpenFilters());
        LoadEventsCommand = new Command(async () => await LoadEvents());
        ViewEventDetailsCommand = new Command<string>(async (eventId) => await ViewEventDetails(eventId));

        UpdateAuthState();
        LoadEventsCommand.Execute(null);
    }

    private bool _isGuestMode = true;
    public bool IsGuestMode
    {
        get => _isGuestMode;
        set => SetProperty(ref _isGuestMode, value);
    }

    private List<Event> _events;
    public List<Event> Events
    {
        get => _events;
        set => SetProperty(ref _events, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand GoToLoginCommand { get; }
    public ICommand CreateEventCommand { get; }
    public ICommand JoinEventCommand { get; }
    public ICommand OpenFiltersCommand { get; }
    public ICommand LoadEventsCommand { get; }
    public ICommand ViewEventDetailsCommand { get; }

    private void OnAuthenticationStateChanged(object sender, EventArgs e)
    {
        UpdateAuthState();
    }

    private void UpdateAuthState()
    {
        IsGuestMode = !_authStateService.IsAuthenticated;
    }

    private async Task GoToLogin()
    {
        await _navigationService.GoToLoginAsync();
    }

    private async Task CreateEvent()
    {
        if (IsGuestMode)
        {
            await GoToLogin();
            return;
        }

        await Shell.Current.GoToAsync("//CreateEventPage");
    }

    public async Task JoinEvent(string eventId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Кнопка 'Я пойду!' нажата для события: {eventId}");

            if (IsGuestMode)
            {
                await GoToLogin();
                return;
            }

            var success = await _dataService.JoinEventAsync(eventId, _authStateService.CurrentUserId);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Успех!", "Вы присоединились к событию!", "OK");
                // Обновляем список событий
                await LoadEvents();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось присоединиться к событию", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка участия: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось присоединиться к событию", "OK");
        }
    }

    private async Task OpenFilters()
    {
        await Shell.Current.GoToAsync("//FilterPage");
    }

    public async Task LoadEvents()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            //System.Diagnostics.Debug.WriteLine("🔄 Загрузка событий...");

            var events = await _dataService.GetEventsAsync();
            Events = events ?? new List<Event>();

            //System.Diagnostics.Debug.WriteLine($"✅ Загружено {Events.Count} событий");
        }
        catch (Exception ex)
        {
            //System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки событий: {ex.Message}");
            Events = new List<Event>();
        }
        finally
        {
            IsLoading = false;
        }
    }
    public async Task ViewEventDetails(string eventId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔄 Переход к событию: {eventId}");

            if (string.IsNullOrEmpty(eventId))
            {
                System.Diagnostics.Debug.WriteLine("❌ eventId пустой!");
                return;
            }

            // Сохраняем eventId в глобальную переменную
            GlobalEventId.EventId = eventId;
            System.Diagnostics.Debug.WriteLine($"💾 Сохранен Global EventId: {eventId}");

            // Навигация
            await Shell.Current.GoToAsync("//EventDetailsPage");
            System.Diagnostics.Debug.WriteLine($"✅ Навигация выполнена");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка навигации: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось открыть событие", "OK");
        }
    }


}