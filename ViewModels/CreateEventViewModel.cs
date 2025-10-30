using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class CreateEventViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IAuthStateService _authStateService;
    private readonly IMapService _mapService;
    private System.Threading.Timer _searchTimer;

    public CreateEventViewModel(IDataService dataService, IAuthStateService authStateService, IMapService mapService)
    {
        _dataService = dataService;
        _authStateService = authStateService;
        _mapService = mapService;

        CreateEventCommand = new Command(async () => await CreateEvent(), () => CanCreateEvent());
        CancelCommand = new Command(async () => await Cancel());
        GetCurrentLocationCommand = new Command(async () => await GetCurrentLocation());
        OpenMapSearchCommand = new Command(async () => await OpenMapSearch());
        SearchAddressCommand = new Command(async () => await SearchAddress());
        SuggestionSelectedCommand = new Command<string>(async (suggestion) => await OnSuggestionSelected(suggestion));

        LoadInterests();

        EventDate = DateTime.Today.AddDays(1);
        EventTime = TimeSpan.FromHours(19);
        MaxParticipants = 20;
    }

    private string _title = "";
    public string Title
    {
        get => _title;
        set
        {
            SetProperty(ref _title, value);
            UpdateCreateCommand();
        }
    }

    private string _description = "";
    public string Description
    {
        get => _description;
        set
        {
            SetProperty(ref _description, value);
            UpdateCreateCommand();
        }
    }

    private List<Interest> _interests;
    public List<Interest> Interests
    {
        get => _interests;
        set => SetProperty(ref _interests, value);
    }

    private Interest _selectedInterest;
    public Interest SelectedInterest
    {
        get => _selectedInterest;
        set
        {
            SetProperty(ref _selectedInterest, value);
            UpdateCreateCommand();
        }
    }

    private DateTime _eventDate;
    public DateTime EventDate
    {
        get => _eventDate;
        set => SetProperty(ref _eventDate, value);
    }

    private TimeSpan _eventTime;
    public TimeSpan EventTime
    {
        get => _eventTime;
        set => SetProperty(ref _eventTime, value);
    }
    private List<string> _addressSuggestions = new List<string>();
    public List<string> AddressSuggestions
    {
        get => _addressSuggestions;
        set => SetProperty(ref _addressSuggestions, value);
    }

    private string _address = "";
    public string Address
    {
        get => _address;
        set
        {
            if (SetProperty(ref _address, value))
            {
                UpdateCreateCommand();
                _ = SearchAddressSuggestions(); // Автопоиск при изменении
            }
        }
    }

    private double? _latitude;
    public double? Latitude
    {
        get => _latitude;
        set => SetProperty(ref _latitude, value);
    }

    private double? _longitude;
    public double? Longitude
    {
        get => _longitude;
        set => SetProperty(ref _longitude, value);
    }

    private int _maxParticipants = 20;
    public int MaxParticipants
    {
        get => _maxParticipants;
        set => SetProperty(ref _maxParticipants, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            SetProperty(ref _isBusy, value);
            UpdateCreateCommand();
        }
    }

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private string _selectionStatus = "";
    public string SelectionStatus
    {
        get => _selectionStatus;
        set => SetProperty(ref _selectionStatus, value);
    }

    private string _selectedSuggestion;
    public string SelectedSuggestion
    {
        get => _selectedSuggestion;
        set
        {
            if (SetProperty(ref _selectedSuggestion, value) && !string.IsNullOrEmpty(value))
            {
                // Показываем статус выбора
                SelectionStatus = "✓ Адрес выбран";

                // Обрабатываем выбор подсказки
                _ = OnSuggestionSelected(value);

                // Через 2 секунды скрываем статус
                Task.Delay(2000).ContinueWith(_ =>
                {
                    MainThread.BeginInvokeOnMainThread(() => SelectionStatus = "");
                });
            }
        }
    }

    public DateTime MinDate => DateTime.Today;
    public DateTime MaxDate => DateTime.Today.AddYears(1);

    public ICommand CreateEventCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand GetCurrentLocationCommand { get; }
    public ICommand OpenMapSearchCommand { get; }
    public ICommand SearchAddressCommand { get; }

    public ICommand SuggestionSelectedCommand { get; }




    private async Task SearchAddress()
    {
        if (string.IsNullOrWhiteSpace(Address))
            return;

        try
        {
            IsBusy = true;

            // Здесь можно добавить поиск координат по адресу (обратное геокодирование)
            var location = await _mapService.GetCoordinatesFromAddressAsync(Address);

            if (location != null)
            {
                Latitude = location.Latitude;
                Longitude = location.Longitude;

                await Application.Current.MainPage.DisplayAlert("Успех!",
                    "Координаты определены по адресу", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка поиска адреса: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanCreateEvent()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(Title) &&
               !string.IsNullOrWhiteSpace(Description) &&
               SelectedInterest != null &&
               !string.IsNullOrWhiteSpace(Address) &&
               MaxParticipants >= 2;
    }

    private void UpdateCreateCommand()
    {
        (CreateEventCommand as Command)?.ChangeCanExecute();
    }

    private async Task GetCurrentLocation()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = "";

            System.Diagnostics.Debug.WriteLine("🎯 Определение местоположения...");

            var location = await _mapService.GetCurrentLocationAsync();

            if (location != null)
            {
                Latitude = location.Latitude;
                Longitude = location.Longitude;

                // Получаем нормальный адрес по координатам
                var address = await _mapService.GetAddressFromCoordinatesAsync(location.Latitude, location.Longitude);
                Address = address;

                // Скрываем подсказки
                AddressSuggestions = new List<string>();

                System.Diagnostics.Debug.WriteLine($"📍 Местоположение определено: {Address}");
                System.Diagnostics.Debug.WriteLine($"📍 Координаты: {Latitude}, {Longitude}");

                await Application.Current.MainPage.DisplayAlert("Успех!",
                    $"Местоположение определено!\n{Address}", "OK");
            }
            else
            {
                ErrorMessage = "Не удалось определить местоположение";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка определения местоположения: {ex.Message}");
            ErrorMessage = "Не удалось определить местоположение";

            // Устанавливаем координаты Москвы по умолчанию с нормальным адресом
            Latitude = 55.7558;
            Longitude = 37.6173;
            Address = "Москва, Россия";
            AddressSuggestions = new List<string>();

            await Application.Current.MainPage.DisplayAlert("Информация",
                "Используется местоположение по умолчанию: Москва", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateEvent()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = "";

            var eventDateTime = EventDate.Add(EventTime);

            if (eventDateTime <= DateTime.Now)
            {
                ErrorMessage = "Дата события должна быть в будущем";
                return;
            }

            // Если координаты не определены, используем Москву по умолчанию
            if (!Latitude.HasValue || !Longitude.HasValue)
            {
                Latitude = 55.7558;
                Longitude = 37.6173;
            }

            var newEvent = new Event
            {
                Title = Title.Trim(),
                Description = Description.Trim(),
                CategoryId = SelectedInterest.Name,
                EventDate = eventDateTime,
                Address = Address.Trim(),
                Latitude = Latitude,
                Longitude = Longitude,
                MaxParticipants = MaxParticipants,
                CreatorId = _authStateService.CurrentUserId,
                CreatorName = await GetCurrentUserNameAsync()
            };

            System.Diagnostics.Debug.WriteLine($"🎯 Создается событие с координатами: {Latitude}, {Longitude}");

            var success = await _dataService.AddEventAsync(newEvent);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Успех!", "Событие создано!", "OK");
                await Shell.Current.GoToAsync("//HomePage");
            }
            else
            {
                ErrorMessage = "Не удалось создать событие";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания события: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<string> GetCurrentUserNameAsync()
    {
        try
        {
            var user = await _dataService.GetUserAsync(_authStateService.CurrentUserId);
            return user?.DisplayName ?? "Организатор";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения имени пользователя: {ex.Message}");
            return "Организатор";
        }
    }

    private async Task Cancel()
    {
        System.Diagnostics.Debug.WriteLine("❌ Выполняется команда Отмена");

        try
        {
            await Shell.Current.GoToAsync("//HomePage");
            System.Diagnostics.Debug.WriteLine("✅ Отмена: переход на главную выполнен");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка при отмене: {ex.Message}");
        }
    }

    private async void LoadInterests()
    {
        try
        {
            Interests = await _dataService.GetInterestsAsync();

            if (Interests == null || Interests.Count == 0)
            {
                Interests = new List<Interest>
                {
                    new Interest { Name = "🎮 Настольные игры" },
                    new Interest { Name = "🎭 Косплей" },
                    new Interest { Name = "🎨 Искусство" },
                    new Interest { Name = "💻 Программирование" },
                    new Interest { Name = "📺 Аниме" },
                    new Interest { Name = "🎵 Музыка" },
                    new Interest { Name = "🍳 Кулинария" },
                    new Interest { Name = "📚 Книги" },
                    new Interest { Name = "🚶‍♂️ Прогулки" },
                    new Interest { Name = "🎬 Кино" }
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки интересов: {ex.Message}");
        }
    }
    private async Task SearchAddressSuggestions()
    {
        if (string.IsNullOrWhiteSpace(Address) || Address.Length < 3)
        {
            AddressSuggestions = new List<string>();
            return;
        }

        try
        {
            var suggestions = await _mapService.GetAddressSuggestionsAsync(Address);
            AddressSuggestions = suggestions;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка поиска подсказок: {ex.Message}");
            AddressSuggestions = new List<string>();
        }
    }

    private async Task OnSuggestionSelected(string suggestion)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Выбрана подсказка: {suggestion}");

            // Устанавливаем адрес в поле ввода
            Address = suggestion;

            // Очищаем подсказки
            AddressSuggestions = new List<string>();

            // Получаем координаты для выбранного адреса
            var location = await _mapService.GetCoordinatesFromAddressAsync(suggestion);

            if (location != null)
            {
                Latitude = location.Latitude;
                Longitude = location.Longitude;
                System.Diagnostics.Debug.WriteLine($"📍 Координаты определены: {Latitude}, {Longitude}");

                // Показываем подтверждение
                SelectionStatus = "✓ Координаты определены";
            }
            else
            {
                SelectionStatus = "⚠ Координаты не определены";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обработки подсказки: {ex.Message}");
            SelectionStatus = "❌ Ошибка определения координат";
        }
    }
    private async Task OpenMapSearch()
    {
        try
        {
            await Application.Current.MainPage.DisplayAlert("Инфо",
                "Функция выбора на карте будет доступна в следующем обновлении", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка открытия карты: {ex.Message}");
        }
    }

}