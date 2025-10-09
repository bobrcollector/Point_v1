using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly IAuthStateService _authStateService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;
    private readonly ISearchService _searchService;
    private readonly FilterStateService _filterStateService;

    public HomeViewModel(IAuthStateService authStateService, IDataService dataService,
                        INavigationService navigationService, ISearchService searchService,
                        FilterStateService filterStateService)
    {
        _authStateService = authStateService;
        _dataService = dataService;
        _navigationService = navigationService;
        _searchService = searchService;
        _filterStateService = filterStateService;

        _authStateService.AuthenticationStateChanged += OnAuthenticationStateChanged;

        // Команды
        GoToLoginCommand = new Command(async () => await GoToLogin());
        CreateEventCommand = new Command(async () => await CreateEvent());
        JoinEventCommand = new Command<string>(async (eventId) => await JoinEvent(eventId));
        OpenFiltersCommand = new Command(async () => await OpenFilters());
        LoadEventsCommand = new Command(async () => await LoadEvents());
        ViewEventDetailsCommand = new Command<string>(async (eventId) => await ViewEventDetails(eventId));
        SearchCommand = new Command(async () => await PerformSearch());
        ClearSearchCommand = new Command(async () => await ClearSearch());
        ClearAllFiltersCommand = new Command(async () => await ClearAllFilters());

        UpdateAuthState();
        LoadEventsCommand.Execute(null);
    }

    // СВОЙСТВА
    private bool _isGuestMode = true;
    public bool IsGuestMode
    {
        get => _isGuestMode;
        set => SetProperty(ref _isGuestMode, value);
    }

    private bool _isAuthenticated;
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
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

    private string _searchQuery = "";
    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    private bool _hasActiveFilters;
    public bool HasActiveFilters
    {
        get => _hasActiveFilters;
        set => SetProperty(ref _hasActiveFilters, value);
    }

    // список активных фильтров для отображения
    private List<string> _activeFilterLabels = new List<string>();
    public List<string> ActiveFilterLabels
    {
        get => _activeFilterLabels;
        set => SetProperty(ref _activeFilterLabels, value);
    }

    private string _emptyViewTitle = "Событий пока нет";
    public string EmptyViewTitle
    {
        get => _emptyViewTitle;
        set => SetProperty(ref _emptyViewTitle, value);
    }

    private string _emptyViewMessage = "Создайте первое событие!";
    public string EmptyViewMessage
    {
        get => _emptyViewMessage;
        set => SetProperty(ref _emptyViewMessage, value);
    }

    // КОМАНДЫ
    public ICommand GoToLoginCommand { get; }
    public ICommand CreateEventCommand { get; }
    public ICommand JoinEventCommand { get; }
    public ICommand OpenFiltersCommand { get; }
    public ICommand LoadEventsCommand { get; }
    public ICommand ViewEventDetailsCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand ClearAllFiltersCommand { get; }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(SearchQuery))
        {
            // Сохраняем поисковый запрос в фильтры
            _filterStateService.SearchText = SearchQuery;
            UpdateFiltersStatus();
        }
    }

    private void OnAuthenticationStateChanged(object sender, EventArgs e)
    {
        UpdateAuthState();
    }

    private void UpdateAuthState()
    {
        IsGuestMode = !_authStateService.IsAuthenticated;
        IsAuthenticated = _authStateService.IsAuthenticated;
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

            // есть ли активные филтры
            if (_filterStateService.HasActiveFilters)
            {
                System.Diagnostics.Debug.WriteLine("🎯 Применяем сохраненные фильтры");

                // применяем фильтры через SearchService
                var filteredEvents = await _searchService.SearchEventsAsync(
                    _filterStateService.SearchText,
                    _filterStateService.SelectedCategory,
                    _filterStateService.SelectedDate
                );

                Events = filteredEvents;
                SearchQuery = _filterStateService.SearchText;
            }
            else
            {
                // Загружаем все события
                var events = await _dataService.GetEventsAsync();
                Events = events ?? new List<Event>();
            }

            UpdateEmptyView();
            UpdateFiltersStatus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки событий: {ex.Message}");
            Events = new List<Event>();
            UpdateEmptyView();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task PerformSearch()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            System.Diagnostics.Debug.WriteLine($"🔍 Выполняется поиск: '{SearchQuery}'");

            // Сохраняем поиск в фильтры
            _filterStateService.SearchText = SearchQuery;

            if (string.IsNullOrWhiteSpace(SearchQuery) && !_filterStateService.HasActiveFilters)
            {
                await LoadEvents();
            }
            else
            {
                // Используем SearchService для поиска с учетом всех фильтров
                var results = await _searchService.SearchEventsAsync(
                    _filterStateService.SearchText,
                    _filterStateService.SelectedCategory,
                    _filterStateService.SelectedDate
                );

                Events = results;
                UpdateEmptyView();
                UpdateFiltersStatus();

                System.Diagnostics.Debug.WriteLine($"✅ Найдено событий: {results.Count}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка поиска: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось выполнить поиск", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ClearSearch()
    {
        SearchQuery = "";
        await LoadEvents();
        System.Diagnostics.Debug.WriteLine("🧹 Поиск очищен");
    }

    private async Task ClearAllFilters()
    {
        _filterStateService.ClearFilters();
        SearchQuery = "";
        await LoadEvents();
        System.Diagnostics.Debug.WriteLine("🧹 Все фильтры очищены");
    }

    private void UpdateFiltersStatus()
    {
        HasActiveFilters = _filterStateService.HasActiveFilters;
        ActiveFilterLabels = _filterStateService.ActiveFilterLabels;

        System.Diagnostics.Debug.WriteLine($"🎯 Активные фильтры: {string.Join(", ", ActiveFilterLabels)}");
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

            GlobalEventId.EventId = eventId;
            await Shell.Current.GoToAsync("//EventDetailsPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка навигации: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось открыть событие", "OK");
        }
    }

    private void UpdateEmptyView()
    {
        if (_filterStateService.HasActiveFilters && (Events == null || !Events.Any()))
        {
            EmptyViewTitle = "Ничего не найдено";
            EmptyViewMessage = "Попробуйте изменить параметры фильтров";
        }
        else if (Events == null || !Events.Any())
        {
            EmptyViewTitle = "Событий пока нет";
            EmptyViewMessage = "Создайте первое событие!";
        }
        else
        {
            EmptyViewTitle = "";
            EmptyViewMessage = "";
        }
    }
}