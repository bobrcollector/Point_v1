using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public ProfileViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;

        SignOutCommand = new Command(async () => await SignOut());
        GoToLoginCommand = new Command(async () => await GoToLogin());

        _authService.AuthStateChanged += OnAuthStateChanged;

        UpdateUserInfo();
    }

    private string _userName = "Пользователь";
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private string _userEmail = "user@example.com";
    public string UserEmail
    {
        get => _userEmail;
        set => SetProperty(ref _userEmail, value);
    }

    public bool IsAuthenticated => _authService.IsAuthenticated;
    public bool IsGuestMode => !_authService.IsAuthenticated;

    public ICommand SignOutCommand { get; }
    public ICommand GoToLoginCommand { get; }

    private async Task SignOut()
    {
        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Выход",
            "Вы уверены, что хотите выйти?",
            "Да", "Отмена");

        if (confirm)
        {
            await _authService.SignOut();
            await Application.Current.MainPage.DisplayAlert("Успех", "Вы вышли из аккаунта", "OK");
        }
    }

    private async Task GoToLogin()
    {
        await _navigationService.GoToLoginAsync();
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        UpdateUserInfo();
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsGuestMode));
    }

    private void UpdateUserInfo()
    {
        if (_authService.IsAuthenticated)
        {

            UserName = "тестовый пользователь";
            UserEmail = "test@mail.ru";
        }
        else
        {
            UserName = "Пользователь";
            UserEmail = "user@mail.ru";
        }
    }
}