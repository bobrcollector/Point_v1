using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class EventDetailsViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IAuthStateService _authStateService;
    private readonly string _eventId;

    public EventDetailsViewModel(IDataService dataService, IAuthStateService authStateService, string eventId)
    {
        _dataService = dataService;
        _authStateService = authStateService;
        _eventId = eventId;

        LoadEventDetailsCommand = new Command(async () => await LoadEventDetails());
        ToggleParticipationCommand = new Command(async () => await ToggleParticipation());
        OpenChatCommand = new Command(async () => await OpenChat());
        GoToLoginCommand = new Command(async () => await GoToLogin());

        // ��������� ������ �������
        LoadEventDetailsCommand.Execute(null);
    }

    private Event _event;
    public Event Event
    {
        get => _event;
        set
        {
            SetProperty(ref _event, value);
            UpdateParticipationState();
        }
    }

    private bool _isParticipating;
    public bool IsParticipating
    {
        get => _isParticipating;
        set
        {
            SetProperty(ref _isParticipating, value);
            UpdateParticipationButton();
        }
    }

    public int ParticipantsCount => Event?.ParticipantIds?.Count ?? 0;

    public bool IsAuthenticated => _authStateService.IsAuthenticated;
    public bool IsGuestMode => !_authStateService.IsAuthenticated;

    private string _participationButtonText = "� �����!";
    public string ParticipationButtonText
    {
        get => _participationButtonText;
        set => SetProperty(ref _participationButtonText, value);
    }

    private Color _participationButtonColor = Colors.Green;
    public Color ParticipationButtonColor
    {
        get => _participationButtonColor;
        set => SetProperty(ref _participationButtonColor, value);
    }

    public ICommand LoadEventDetailsCommand { get; }
    public ICommand ToggleParticipationCommand { get; }
    public ICommand OpenChatCommand { get; }
    public ICommand GoToLoginCommand { get; }

    private async Task LoadEventDetails()
    {
        try
        {
            Event = await _dataService.GetEventAsync(_eventId);

            if (Event == null)
            {
                await Application.Current.MainPage.DisplayAlert("������", "������� �� �������", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            UpdateParticipationState();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("������", "�� ������� ��������� �������", "OK");
            System.Diagnostics.Debug.WriteLine($"������ �������� �������: {ex.Message}");
        }
    }

    private void UpdateParticipationState()
    {
        if (Event == null || !_authStateService.IsAuthenticated)
        {
            IsParticipating = false;
            return;
        }

        IsParticipating = Event.ParticipantIds.Contains(_authStateService.CurrentUserId);
    }

    private void UpdateParticipationButton()
    {
        if (IsParticipating)
        {
            ParticipationButtonText = "� �� �����";
            ParticipationButtonColor = Color.FromArgb("#FF6B6B"); // �������
        }
        else
        {
            ParticipationButtonText = "� �����!";
            ParticipationButtonColor = Color.FromArgb("#4CAF50"); // �������
        }
    }

    private async Task ToggleParticipation()
    {
        if (!_authStateService.IsAuthenticated)
        {
            await GoToLogin();
            return;
        }

        try
        {
            bool success;

            if (IsParticipating)
            {
                success = await _dataService.LeaveEventAsync(_eventId, _authStateService.CurrentUserId);
                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("�����", "�� ������ �� ���������� � �������", "OK");
                }
            }
            else
            {
                success = await _dataService.JoinEventAsync(_eventId, _authStateService.CurrentUserId);
                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("�����", "�� �������������� � �������!", "OK");
                }
            }

            if (success)
            {
                // ��������� ������ �������
                await LoadEventDetails();
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("������", "�� ������� �������� �������", "OK");
            System.Diagnostics.Debug.WriteLine($"������ ��������� �������: {ex.Message}");
        }
    }

    private async Task OpenChat()
    {
        // ��������� �������� ��� ����
        await Application.Current.MainPage.DisplayAlert("���", "������� ���� ����� ����������� �����", "OK");
    }

    private async Task GoToLogin()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}