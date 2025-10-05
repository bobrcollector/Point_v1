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

        System.Diagnostics.Debug.WriteLine("AuthViewModel ������!");
        System.Diagnostics.Debug.WriteLine($"AuthService: {authService != null}");
        System.Diagnostics.Debug.WriteLine($"NavigationService: {navigationService != null}");

        // ������� ��� ����
        LoginCommand = new Command(async () => await Login(), () => CanLogin());
        RegisterCommand = new Command(async () => await Register(), () => CanRegister());

        // ������� ��� ��������� ����� ����������
        GoToLoginPageCommand = new Command(async () => await GoToLoginPage());
        GoToRegisterPageCommand = new Command(async () => await GoToRegisterPage());

        // ������������� �� ��������� ��������� ��������������
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

    // �������
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
                SuccessMessage = "���� �������� �������!";
                // ��������� ���������� ������������� ����� AuthStateChanged
            }
            else
            {
                ErrorMessage = "������ �����. ��������� email � ������.\n(���������: ������ '123456')";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"������: {ex.Message}";
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

            // �������� ������������� ������
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "������ �� ���������";
                return;
            }

            // �������� ����� ������
            if (Password.Length < 6)
            {
                ErrorMessage = "������ ������ ��������� ������� 6 ��������";
                return;
            }

            var success = await _authService.CreateUser(Email, Password, DisplayName);

            if (success)
            {
                SuccessMessage = "����������� ������ �������!";
                // ��������� ���������� ������������� ����� AuthStateChanged
            }
            else
            {
                ErrorMessage = "������ �����������. ��������� ��������� ������.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"������: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    private async Task GoToLoginPage()
    {
        System.Diagnostics.Debug.WriteLine("������� �� LoginPage");
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private async Task GoToRegisterPage()
    {
        System.Diagnostics.Debug.WriteLine("������� �� RegisterPage");
        await Shell.Current.GoToAsync("//RegisterPage");
    }

    private async void OnAuthStateChanged(object sender, EventArgs e)
    {
        // ��� �������� �������������� ��������� �� �������
        if (_authService.IsAuthenticated)
        {
            await _navigationService.GoToHomeAsync();
        }
    }
}