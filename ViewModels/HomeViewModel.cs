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
    private readonly MapViewStateService _mapViewStateService;

    public HomeViewModel(IAuthStateService authStateService, IDataService dataService,
                        INavigationService navigationService, ISearchService searchService,
                        FilterStateService filterStateService, IMapService mapService,
                        MapHtmlService mapHtmlService, MapViewStateService mapViewStateService)
    {
        _authStateService = authStateService;
        _dataService = dataService;
        _navigationService = navigationService;
        _searchService = searchService;
        _filterStateService = filterStateService;
        _mapService = mapService;
        _mapHtmlService = mapHtmlService;
        _mapViewStateService = mapViewStateService;

        _authStateService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        
        _filterStateService.FiltersChanged += OnFiltersChanged;
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
        _ = LoadUserInterests().ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadEventsCommand.Execute(null);
            });
        });
    }

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
        _mapViewStateService.SetMapViewActive(true);

        OnPropertyChanged(nameof(IsListView));
        OnPropertyChanged(nameof(IsMapView));

        if (_filterStateService.HasActiveFilters)
        {
            System.Diagnostics.Debug.WriteLine("🗺️ SwitchToMap: применяем активные фильтры");
            await ApplyFiltersToMap();
        }
        else
        {
            await LoadMapEvents();
        }
        
        System.Diagnostics.Debug.WriteLine("🎯 SwitchToMap завершен");
    }

    private void SwitchToList()
    {
        System.Diagnostics.Debug.WriteLine("🎯 SwitchToList начат");
        IsMapView = false;
        _mapViewStateService.SetMapViewActive(false);

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

            MapHtmlContent = _mapHtmlService.GenerateMapHtml(mapEvents, location.Latitude, location.Longitude, showUserLocation: true);
            System.Diagnostics.Debug.WriteLine($"🗺️ HTML карты сгенерирован, длина: {MapHtmlContent?.Length ?? 0}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки событий на карту: {ex.Message}");
            MapHtmlContent = _mapHtmlService.GenerateMapHtml(new List<MapEvent>(), showUserLocation: true);
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine("🗺️ LoadMapEvents завершен");
        }
    }

    public async Task LoadMapEventsWithFocus(List<Event> allEvents, string focusedEventId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🗺️ LoadMapEventsWithFocus начат для события: {focusedEventId}");
            IsLoading = true;

            var location = await _mapService.GetCurrentLocationAsync();
            System.Diagnostics.Debug.WriteLine($"📍 Получена локация: {location.Latitude}, {location.Longitude}");

            var mapEvents = new List<MapEvent>();
            foreach (var eventItem in allEvents)
            {
                if (eventItem.IsBlocked || !eventItem.IsActive)
                    continue;

                if (eventItem.Latitude.HasValue && eventItem.Longitude.HasValue)
                {
                    mapEvents.Add(new MapEvent
                    {
                        EventId = eventItem.Id,
                        Title = eventItem.Title,
                        Description = eventItem.ShortDescription,
                        Address = eventItem.Address,
                        CategoryId = eventItem.CategoryId,
                        EventDate = eventItem.EventDate,
                        ParticipantsCount = eventItem.ParticipantsCount,
                        Latitude = eventItem.Latitude.Value,
                        Longitude = eventItem.Longitude.Value
                    });
                }
            }

            System.Diagnostics.Debug.WriteLine($"🎯 Найдено событий для карты: {mapEvents?.Count ?? 0}");

            MapHtmlContent = _mapHtmlService.GenerateMapHtmlWithCenter(mapEvents, focusedEventId, location.Latitude, location.Longitude, showUserLocation: false);
            System.Diagnostics.Debug.WriteLine($"🗺️ HTML карты сгенерирован, длина: {MapHtmlContent?.Length ?? 0}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки событий на карту: {ex.Message}");
            MapHtmlContent = _mapHtmlService.GenerateMapHtml(new List<MapEvent>());
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine("🗺️ LoadMapEventsWithFocus завершен");
        }
    }

    public override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(SearchQuery))
        {
            _filterStateService.SearchText = SearchQuery;
            UpdateFiltersStatus();
        }
    }

    private async void OnAuthenticationStateChanged(object sender, EventArgs e)
    {
        UpdateAuthState();
        await LoadUserInterests();
        await LoadEvents();
    }

    private void OnFiltersChanged(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("🎯 OnFiltersChanged вызван");
        UpdateFiltersStatus();
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
        System.Diagnostics.Debug.WriteLine($"🗺️ OpenFilters: текущая вкладка - {(IsMapView ? "Карта" : "Список")}");
        
        _mapViewStateService.SetMapViewActive(IsMapView);
        System.Diagnostics.Debug.WriteLine($"💾 MapViewStateService обновлена: IsMapViewActive = {IsMapView}");
        
        await Shell.Current.GoToAsync(nameof(FilterPage));
    }

    public async Task LoadEvents()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;

            if (IsAuthenticated)
            {
                await LoadUserInterests();
            }

            bool shouldBeMapView = _mapViewStateService.IsMapViewActive;
            System.Diagnostics.Debug.WriteLine($"🗺️ LoadEvents: shouldBeMapView = {shouldBeMapView}, IsMapView = {IsMapView}, HasActiveFilters = {_filterStateService.HasActiveFilters}");

            if (shouldBeMapView && !IsMapView)
            {
                System.Diagnostics.Debug.WriteLine("🗺️ Переключение на карту после возврата с фильтров");
                await SwitchToMap();
                
                if (_filterStateService.HasActiveFilters)
                {
                    await ApplyFiltersToMap();
                }
                
                return; 
            }

            if (IsMapView)
            {
                if (_filterStateService.HasActiveFilters)
                {
                    System.Diagnostics.Debug.WriteLine("🗺️ На карте есть активные фильтры - применяем их");
                    await ApplyFiltersToMap();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("🗺️ На карте нет фильтров - загружаем все события");
                    await LoadMapEvents();
                }
            }
            else
            {
                List<Event> events;

                if (_filterStateService.HasActiveFilters)
                {
                    System.Diagnostics.Debug.WriteLine("📋 В списке есть активные фильтры - применяем их");
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
                    System.Diagnostics.Debug.WriteLine("📋 В списке нет фильтров - загружаем все события");
                    events = await _dataService.GetEventsAsync();
                }

                if (events != null)
                {
                    var filteredEvents = events.Where(e => e.EventDate > DateTime.Now && !e.IsBlocked && e.IsActive).ToList();
                    UpdateEventsRelevance(filteredEvents);
                    var sortedEvents = SortEventsByRelevance(filteredEvents);
                    foreach (var evt in sortedEvents.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"📋 Событие '{evt.Title}': CategoryIds={string.Join(", ", evt.CategoryIds ?? new List<string>())}, DisplayCategories={string.Join(", ", evt.DisplayCategories)}");
                    }
                    
                    Events = sortedEvents;
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
            _filterStateService.SearchText = SearchQuery;

            if (string.IsNullOrWhiteSpace(SearchQuery) && !_filterStateService.HasActiveFilters)
            {
                await LoadEvents();
            }
            else
            {
                var results = await _searchService.SearchEventsAsync(
                    _filterStateService.SearchText,
                    _filterStateService.SelectedCategory,
                    _filterStateService.SelectedDate
                );

                if (IsMapView && results.Count > 0)
                {
                    await LoadMapEventsWithFocus(results, results[0].Id);
                }
                else
                {
                    Events = results;
                }

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

    public async Task ApplyFiltersToMap()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🗺️ Применение фильтров к карте");
            IsLoading = true;
            List<Event> filteredEvents;

            if (_filterStateService.HasActiveFilters)
            {
                filteredEvents = await _searchService.SearchEventsAsync(
                    _filterStateService.SearchText,
                    _filterStateService.SelectedCategory,
                    _filterStateService.SelectedDate
                );
            }
            else
            {
                filteredEvents = await _dataService.GetEventsAsync();
            }
            if (filteredEvents != null)
            {
                filteredEvents = filteredEvents.Where(e => e.EventDate > DateTime.Now && !e.IsBlocked && e.IsActive).ToList();
            }
            else
            {
                filteredEvents = new List<Event>();
            }

            System.Diagnostics.Debug.WriteLine($"🎯 Найдено отфильтрованных событий: {filteredEvents.Count}");
            if (filteredEvents.Count > 0)
            {
                await LoadMapEventsWithFocus(filteredEvents, filteredEvents[0].Id);
            }
            else
            {
                var location = await _mapService.GetCurrentLocationAsync();
                MapHtmlContent = _mapHtmlService.GenerateMapHtml(new List<MapEvent>(), location.Latitude, location.Longitude, showUserLocation: true);
                UpdateEmptyView();
            }

            UpdateFiltersStatus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка применения фильтров к карте: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось применить фильтры", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateFiltersStatus()
    {
        try
        {
            var oldHasActiveFilters = HasActiveFilters;
            var oldLabels = ActiveFilterLabels;
            
            HasActiveFilters = _filterStateService.HasActiveFilters;
            ActiveFilterLabels = new List<string>(_filterStateService.ActiveFilterLabels);

            System.Diagnostics.Debug.WriteLine($"🎯 UpdateFiltersStatus: HasActiveFilters {oldHasActiveFilters} -> {HasActiveFilters}");
            System.Diagnostics.Debug.WriteLine($"🎯 Активные фильтры: {string.Join(", ", ActiveFilterLabels ?? new List<string>())}");
            if (oldHasActiveFilters != HasActiveFilters)
            {
                OnPropertyChanged(nameof(HasActiveFilters));
                System.Diagnostics.Debug.WriteLine($"🎯 HasActiveFilters PropertyChanged вызван");
            }
            
            if (oldLabels == null || ActiveFilterLabels == null || 
                oldLabels.Count != ActiveFilterLabels.Count || 
                !oldLabels.SequenceEqual(ActiveFilterLabels))
            {
                OnPropertyChanged(nameof(ActiveFilterLabels));
                System.Diagnostics.Debug.WriteLine($"🎯 ActiveFilterLabels PropertyChanged вызван");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в UpdateFiltersStatus: {ex.Message}");
        }
    }

    public async Task ViewEventDetails(string eventId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Переход к событию из Home: {eventId}");

            var eventItem = await _dataService.GetEventAsync(eventId);
            if (eventItem == null)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Событие не найдено", "OK");
                return;
            }

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

    private void UpdateEventsRelevance(List<Event> events)
    {
        if (!IsAuthenticated || UserInterestIds.Count == 0)
        {
            foreach (var eventItem in events)
            {
                eventItem.IsRelevant = false;
            }
            return;
        }
        var allInterests = GetDefaultInterests();

        System.Diagnostics.Debug.WriteLine($"🎯 Сравниваем события с интересами пользователя: {UserInterestIds.Count} интересов");

        foreach (var eventItem in events)
        {
            try
            {
                var userInterests = allInterests
                    .Where(interest => UserInterestIds.Contains(interest.Id))
                    .Select(interest => interest.Name)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"🎯 Интересы пользователя: {string.Join(", ", userInterests)}");
                System.Diagnostics.Debug.WriteLine($"🎯 Категория события: {eventItem.CategoryId}");
                var eventCategories = new List<string>();
                if (!string.IsNullOrWhiteSpace(eventItem.CategoryId))
                {
                    eventCategories.Add(eventItem.CategoryId);
                }
                if (eventItem.CategoryIds != null && eventItem.CategoryIds.Count > 0)
                {
                    eventCategories.AddRange(eventItem.CategoryIds.Where(c => !string.IsNullOrWhiteSpace(c)));
                }
                
                eventItem.IsRelevant = eventCategories.Any(eventCategory =>
                    userInterests.Any(userInterest =>
                        eventCategory.Contains(userInterest) || userInterest.Contains(eventCategory)));

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
        new Interest { Id = "15", Name = "🚗 Автомобили" },
        new Interest { Id = "16", Name = "🏥 Медицина" },
        new Interest { Id = "17", Name = "📌 Прочее" }
    };
    }

    private async Task ClearSearch()
    {
        SearchQuery = "";
        
        if (IsMapView)
        {
            await LoadMapEvents();
        }
        else
        {
            await LoadEvents();
        }
        System.Diagnostics.Debug.WriteLine("🧹 Поиск очищен");
    }

    private async Task ClearAllFilters()
    {
        System.Diagnostics.Debug.WriteLine("🧹 Начинаем очистку всех фильтров");
        
        _filterStateService.ClearFilters();
        SearchQuery = "";
        
        System.Diagnostics.Debug.WriteLine($"🧹 Фильтры очищены. IsMapView={IsMapView}, HasActiveFilters={_filterStateService.HasActiveFilters}");
        UpdateFiltersStatus();
        
        if (IsMapView)
        {
            System.Diagnostics.Debug.WriteLine("🧹 На карте - загружаем все события без фильтров");
            await LoadMapEvents();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("🧹 В списке - загружаем все события без фильтров");
            await LoadEvents();
        }
        
        System.Diagnostics.Debug.WriteLine("🧹 Все фильтры очищены");
    }
}