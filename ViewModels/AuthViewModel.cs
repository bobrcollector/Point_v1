using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class AuthViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public AuthViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;

        System.Diagnostics.Debug.WriteLine("AuthViewModel создан!");
        System.Diagnostics.Debug.WriteLine($"AuthService: {authService != null}");
        System.Diagnostics.Debug.WriteLine($"NavigationService: {navigationService != null}");

        // Команды для форм
        LoginCommand = new Command(async () => await Login(), () => CanLogin());
        RegisterCommand = new Command(async () => await Register(), () => CanRegister());

        // Команды для навигации между страницами
        GoToLoginPageCommand = new Command(async () => await GoToLoginPage());
        GoToRegisterPageCommand = new Command(async () => await GoToRegisterPage());

        // Подписываемся на изменение состояния аутентификации
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    private string _email = "";
    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            UpdateCommands();
        }
    }

    private string _password = "";
    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            UpdateCommands();
        }
    }

    private string _displayName = "";
    public string DisplayName
    {
        get => _displayName;
        set
        {
            SetProperty(ref _displayName, value);
            UpdateCommands();
        }
    }

    private string _confirmPassword = "";
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            SetProperty(ref _confirmPassword, value);
            UpdateCommands();
        }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            SetProperty(ref _isBusy, value);
            UpdateCommands();
        }
    }

    private string _errorMessage = "";
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private string _successMessage = "";
    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    // Команды
    public Command LoginCommand { get; }
    public Command RegisterCommand { get; }
    public ICommand GoToLoginPageCommand { get; }
    public ICommand GoToRegisterPageCommand { get; }

    private bool CanLogin()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(Password);
    }

    private bool CanRegister()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(Password) &&
               !string.IsNullOrWhiteSpace(DisplayName) &&
               Password == ConfirmPassword;
    }

    private void UpdateCommands()
    {
        LoginCommand.ChangeCanExecute();
        RegisterCommand.ChangeCanExecute();
    }

    private void ClearErrors()
    {
        ErrorMessage = "";
        SuccessMessage = "";
    }

    public async Task Login()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearErrors();

            var success = await _authService.SignIn(Email, Password);

            if (success)
            {
                SuccessMessage = "Вход выполнен успешно!";
                // Навигация произойдет автоматически через AuthStateChanged
            }
            else
            {
                ErrorMessage = "Ошибка входа. Проверьте email и пароль.\n(Подсказка: пароль '123456')";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task Register()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearErrors();

            // Проверка подтверждения пароля
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }

            // Проверка длины пароля
            if (Password.Length < 6)
            {
                ErrorMessage = "Пароль должен содержать минимум 6 символов";
                return;
            }

            var success = await _authService.CreateUser(Email, Password, DisplayName);

            if (success)
            {
                SuccessMessage = "Регистрация прошла успешно!";
                // Навигация произойдет автоматически через AuthStateChanged
            }
            else
            {
                ErrorMessage = "Ошибка регистрации. Проверьте введенные данные.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    private async Task GoToLoginPage()
    {
        System.Diagnostics.Debug.WriteLine("Переход на LoginPage");
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private async Task GoToRegisterPage()
    {
        System.Diagnostics.Debug.WriteLine("Переход на RegisterPage");
        await Shell.Current.GoToAsync("//RegisterPage");
    }

    private async void OnAuthStateChanged(object sender, EventArgs e)
    {
        // При успешной аутентификации переходим на главную
        if (_authService.IsAuthenticated)
        {
            await _navigationService.GoToHomeAsync();
        }
    }
}