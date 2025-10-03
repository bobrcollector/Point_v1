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
        CancelCommand = new Command(async () => await Cancel());

        // ��������� ��������
        LoadInterests();

        // ������������� �������� �� ���������
        EventDate = DateTime.Today.AddDays(1);
        EventTime = TimeSpan.FromHours(18); // 18:00 �� ���������
    }

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            SetProperty(ref _title, value);
            CreateEventCommand.ChangeCanExecute();
        }
    }

    private string _description;
    public string Description
    {
        get => _description;
        set
        {
            SetProperty(ref _description, value);
            CreateEventCommand.ChangeCanExecute();
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
            CreateEventCommand.ChangeCanExecute();
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

    private string _address;
    public string Address
    {
        get => _address;
        set
        {
            SetProperty(ref _address, value);
            CreateEventCommand.ChangeCanExecute();
        }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public DateTime Today => DateTime.Today;

    public Command CreateEventCommand { get; }
    public Command CancelCommand { get; }

    private bool CanCreateEvent()
    {
        return !string.IsNullOrWhiteSpace(Title) &&
               !string.IsNullOrWhiteSpace(Description) &&
               SelectedInterest != null &&
               !string.IsNullOrWhiteSpace(Address) &&
               !IsBusy;
    }

    private async Task CreateEvent()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // �������� ���� � �����
            var eventDateTime = EventDate.Add(EventTime);

            var newEvent = new Event
            {
                Title = Title,
                Description = Description,
                CategoryId = SelectedInterest.Name, // ���������� ��� ��� ���������
                EventDate = eventDateTime,
                Address = Address,
                CreatorId = _authStateService.CurrentUserId,
                ParticipantIds = new List<string> { _authStateService.CurrentUserId }
            };

            var success = await _dataService.AddEventAsync(newEvent);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("�����", "������� �������!", "OK");
                await Shell.Current.GoToAsync(".."); // ������������ �����
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("������", "�� ������� ������� �������", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("������", $"������: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"������ �������� �������: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void LoadInterests()
    {
        try
        {
            Interests = await _dataService.GetInterestsAsync();

            // ���� ��������� ���, ������� ��������
            if (Interests == null || Interests.Count == 0)
            {
                Interests = new List<Interest>
                {
                    new Interest { Name = "���������� ����" },
                    new Interest { Name = "�������" },
                    new Interest { Name = "���������" },
                    new Interest { Name = "����������������" },
                    new Interest { Name = "�������" }
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"������ �������� ���������: {ex.Message}");
        }
    }
}