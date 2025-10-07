using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class CreateEventViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IAuthStateService _authStateService;

    public CreateEventViewModel(IDataService dataService, IAuthStateService authStateService)
    {
        _dataService = dataService;
        _authStateService = authStateService;

        CreateEventCommand = new Command(async () => await CreateEvent(), () => CanCreateEvent());
        CancelCommand = new Command(async () => await Cancel()); // ИСПРАВЛЕНА КОМАНДА

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

    private string _address = "";
    public string Address
    {
        get => _address;
        set
        {
            SetProperty(ref _address, value);
            UpdateCreateCommand();
        }
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

    public DateTime MinDate => DateTime.Today;
    public DateTime MaxDate => DateTime.Today.AddYears(1);

    public ICommand CreateEventCommand { get; }
    public ICommand CancelCommand { get; }

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

            var newEvent = new Event
            {
                Title = Title.Trim(),
                Description = Description.Trim(),
                CategoryId = SelectedInterest.Name,
                EventDate = eventDateTime,
                Address = Address.Trim(),
                MaxParticipants = MaxParticipants,
                CreatorId = _authStateService.CurrentUserId,
                CreatorName = "Тестовый Организатор" 
            };

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
            System.Diagnostics.Debug.WriteLine($"Ошибка создания события: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task Cancel()
    {
        System.Diagnostics.Debug.WriteLine("❌ Выполняется команда Отмена");

        try
        {
            // ПРОБУЕМ РАЗНЫЕ СПОСОБЫ
            await Shell.Current.GoToAsync("//HomePage");
            System.Diagnostics.Debug.WriteLine("✅ Отмена: переход на главную выполнен");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка при отмене: {ex.Message}");

            // Альтернативный способ
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
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
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки интересов: {ex.Message}");
        }
    }
}