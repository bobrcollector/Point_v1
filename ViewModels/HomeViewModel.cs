using Point_v1.Models;
using Point_v1.Services;
using Point_v1.Views;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly IAuthStateService _authStateService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;
    private readonly ISearchService _searchService;
    private readonly FilterStateService _filterStateService;
    private readonly IMapService _mapService;
    private readonly MapHtmlService _mapHtmlService;

    public HomeViewModel(IAuthStateService authStateService, IDataService dataService,
                        INavigationService navigationService, ISearchService searchService,
                        FilterStateService filterStateService, IMapService mapService,
                        MapHtmlService mapHtmlService)
    {
        _authStateService = authStateService;
        _dataService = dataService;
        _navigationService = navigationService;
        _searchService = searchService;
        _filterStateService = filterStateService;
        _mapService = mapService;
        _mapHtmlService = mapHtmlService;

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


        SwitchToMapCommand = new Command(async () =>
        {
            System.Diagnostics.Debug.WriteLine("🎯 SwitchToMapCommand ВЫЗВАН!");
            await SwitchToMap();
        });

        SwitchToListCommand = new Command(() =>
        {
            System.Diagnostics.Debug.WriteLine("🎯 SwitchToListCommand ВЫЗВАН!");
            SwitchToList();
        });

        System.Diagnostics.Debug.WriteLine($"🎯 HomeViewModel создан: " +
    $"MapService: {_mapService != null}, " +
    $"MapHtmlService: {_mapHtmlService != null}");

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

    private bool _isMapView = false;
    public bool IsMapView
    {
        get => _isMapView;
        set => SetProperty(ref _isMapView, value);
    }

    public bool IsListView => !IsMapView;

    private string _mapHtmlContent;
    public string MapHtmlContent
    {
        get => _mapHtmlContent;
        set => SetProperty(ref _mapHtmlContent, value);
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
    public ICommand SwitchToMapCommand { get; }
    public ICommand SwitchToListCommand { get; }


    private async Task SwitchToMap()
    {
        System.Diagnostics.Debug.WriteLine("🎯 SwitchToMap начат");
        IsMapView = true;

        // Принудительно обновляем свойства
        OnPropertyChanged(nameof(IsListView));
        OnPropertyChanged(nameof(IsMapView));

        await LoadMapEvents();
        System.Diagnostics.Debug.WriteLine("🎯 SwitchToMap завершен");
    }

    private void SwitchToList()
    {
        System.Diagnostics.Debug.WriteLine("🎯 SwitchToList начат");
        IsMapView = false;

        // Принудительно обновляем свойства
        OnPropertyChanged(nameof(IsListView));
        OnPropertyChanged(nameof(IsMapView));

        System.Diagnostics.Debug.WriteLine("🎯 SwitchToList завершен");
    }
    public async Task LoadMapEvents()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🗺️ LoadMapEvents начат");
            IsLoading = true;

            var location = await _mapService.GetCurrentLocationAsync();
            System.Diagnostics.Debug.WriteLine($"📍 Получена локация: {location.Latitude}, {location.Longitude}");

            var mapEvents = await _mapService.GetEventsNearbyAsync(location);
            System.Diagnostics.Debug.WriteLine($"🎯 Найдено событий для карты: {mapEvents?.Count ?? 0}");

            // Генерируем HTML с картой
            MapHtmlContent = _mapHtmlService.GenerateMapHtml(mapEvents, location.Latitude, location.Longitude);
            System.Diagnostics.Debug.WriteLine($"🗺️ HTML карты сгенерирован, длина: {MapHtmlContent?.Length ?? 0}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки событий на карту: {ex.Message}");
            // Генерируем карту по умолчанию
            MapHtmlContent = _mapHtmlService.GenerateMapHtml(new List<MapEvent>());
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine("🗺️ LoadMapEvents завершен");
        }
    }


    public override void OnPropertyChanged(string propertyName = null)
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

            // ЗАГРУЖАЕМ ИНТЕРЕСЫ ПОЛЬЗОВАТЕЛЯ
            if (IsAuthenticated)
            {
                await LoadUserInterests();
            }

            if (IsMapView)
            {
                await LoadMapEvents();
            }
            else
            {
                List<Event> events;

                if (_filterStateService.HasActiveFilters)
                {
                    var filteredEvents = await _searchService.SearchEventsAsync(
                        _filterStateService.SearchText,
                        _filterStateService.SelectedCategory,
                        _filterStateService.SelectedDate
                    );
                    events = filteredEvents;
                    SearchQuery = _filterStateService.SearchText;
                }
                else
                {
                    events = await _dataService.GetEventsAsync();
                }

                // ФИЛЬТРАЦИЯ: убираем прошедшие события
                if (events != null)
                {
                    var filteredEvents = events.Where(e => e.EventDate > DateTime.Now).ToList();

                    // ОБНОВЛЯЕМ РЕЛЕВАНТНОСТЬ СОБЫТИЙ
                    UpdateEventsRelevance(filteredEvents);

                    // СОРТИРУЕМ ПО РЕЛЕВАНТНОСТИ
                    Events = SortEventsByRelevance(filteredEvents);
                }
                else
                {
                    Events = new List<Event>();
                }
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

    private List<Event> SortEventsByRelevance(List<Event> events)
    {
        if (!IsAuthenticated || UserInterestIds.Count == 0) return events;

        return events.OrderByDescending(e => e.IsRelevant)
                    .ThenBy(e => e.EventDate)
                    .ToList();
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
            System.Diagnostics.Debug.WriteLine($"🎯 Переход к событию из Home: {eventId}");

            // Проверяем существование события
            var eventItem = await _dataService.GetEventAsync(eventId);
            if (eventItem == null)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Событие не найдено", "OK");
                return;
            }

            // ИСПОЛЬЗУЕМ ПРОСТО СТРОКУ ВМЕСТО nameof()
            await Shell.Current.GoToAsync($"{nameof(EventDetailsPage)}?eventId={eventId}");
            System.Diagnostics.Debug.WriteLine($"✅ Успешный переход к событию из Home");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка перехода к событию: {ex.Message}");
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

    private List<string> _userInterestIds = new List<string>();
    public List<string> UserInterestIds
    {
        get => _userInterestIds;
        set => SetProperty(ref _userInterestIds, value);
    }

    private async Task LoadUserInterests()
    {
        if (!IsAuthenticated) return;

        try
        {
            var user = await _dataService.GetUserAsync(_authStateService.CurrentUserId);
            if (user?.InterestIds != null)
            {
                UserInterestIds = user.InterestIds;

                // ОТЛАДОЧНАЯ ИНФОРМАЦИЯ
                var allInterests = GetDefaultInterests();
                var userInterestNames = allInterests
                    .Where(i => UserInterestIds.Contains(i.Id))
                    .Select(i => i.Name)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"🎯 Загружено интересов пользователя: {UserInterestIds.Count}");
                System.Diagnostics.Debug.WriteLine($"🎯 Названия интересов: {string.Join(", ", userInterestNames)}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки интересов пользователя: {ex.Message}");
        }
    }

    // Метод для проверки, подходит ли событие по интересам
    private void UpdateEventsRelevance(List<Event> events)
    {
        if (!IsAuthenticated || UserInterestIds.Count == 0)
        {
            // Сбрасываем релевантность если пользователь не авторизован
            foreach (var eventItem in events)
            {
                eventItem.IsRelevant = false;
            }
            return;
        }

        // ЗАГРУЖАЕМ ВСЕ ИНТЕРЕСЫ ДЛЯ СРАВНЕНИЯ
        var allInterests = GetDefaultInterests(); // Используем тот же список что в CreateEvent

        System.Diagnostics.Debug.WriteLine($"🎯 Сравниваем события с интересами пользователя: {UserInterestIds.Count} интересов");

        foreach (var eventItem in events)
        {
            try
            {
                // Находим интерес по ID из UserInterestIds
                var userInterests = allInterests
                    .Where(interest => UserInterestIds.Contains(interest.Id))
                    .Select(interest => interest.Name)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"🎯 Интересы пользователя: {string.Join(", ", userInterests)}");
                System.Diagnostics.Debug.WriteLine($"🎯 Категория события: {eventItem.CategoryId}");

                // Сравниваем категорию события с названиями интересов пользователя
                eventItem.IsRelevant = userInterests.Any(userInterest =>
                    eventItem.CategoryId?.Contains(userInterest) == true ||
                    userInterest.Contains(eventItem.CategoryId ?? ""));

                System.Diagnostics.Debug.WriteLine($"🎯 Событие '{eventItem.Title}': релевантно = {eventItem.IsRelevant}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка проверки релевантности: {ex.Message}");
                eventItem.IsRelevant = false;
            }
        }
    }
    private List<Interest> GetDefaultInterests()
    {
        return new List<Interest>
    {
        new Interest { Id = "1", Name = "🎲 Настольные игры" },
        new Interest { Id = "2", Name = "🎭 Косплей" },
        new Interest { Id = "3", Name = "🎨 Искусство" },
        new Interest { Id = "4", Name = "💻 Программирование" },
        new Interest { Id = "5", Name = "📺 Аниме" },
        new Interest { Id = "6", Name = "📚 Комиксы" },
        new Interest { Id = "7", Name = "🎬 Кино" },
        new Interest { Id = "8", Name = "🎵 Музыка" },
        new Interest { Id = "9", Name = "⚽ Спорт" },
        new Interest { Id = "10", Name = "✈️ Путешествия" },
        new Interest { Id = "11", Name = "🍳 Кулинария" },
        new Interest { Id = "12", Name = "📸 Фотография" },
        new Interest { Id = "13", Name = "🎮 Видеоигры" },
        new Interest { Id = "14", Name = "📖 Книги" },
        new Interest { Id = "15", Name = "🚗 Автомобили" }
    };
    }
}