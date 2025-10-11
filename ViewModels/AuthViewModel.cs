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
            ValidatePassword();
            ValidateConfirmPassword();
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
            ValidateConfirmPassword();
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

    // ����� �������� ��� ���������
    private string _passwordValidationMessage = "";
    public string PasswordValidationMessage
    {
        get => _passwordValidationMessage;
        set => SetProperty(ref _passwordValidationMessage, value);
    }

    private string _confirmPasswordValidationMessage = "";
    public string ConfirmPasswordValidationMessage
    {
        get => _confirmPasswordValidationMessage;
        set => SetProperty(ref _confirmPasswordValidationMessage, value);
    }

    private bool _passwordsMatch = false;
    public bool PasswordsMatch
    {
        get => _passwordsMatch;
        set => SetProperty(ref _passwordsMatch, value);
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
        PasswordValidationMessage = "";
        ConfirmPasswordValidationMessage = "";
    }

    public async Task Login()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearErrors();

            // ��������� email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "������� email";
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "������� ���������� email �����";
                return;
            }

            // ��������� ������
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "������� ������";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "������ ������ ��������� ������� 6 ��������";
                return;
            }

            var success = await _authService.SignIn(Email, Password);

            if (success)
            {
                SuccessMessage = "���� �������� �������!";
                // ��������� ���������� ������������� ����� AuthStateChanged
            }
            else
            {
                ErrorMessage = "�������� email ��� ������";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = GetUserFriendlyErrorMessage(ex);
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

            // ��������� �����
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                ErrorMessage = "������� ��� ������������";
                return;
            }

            if (DisplayName.Length < 2)
            {
                ErrorMessage = "��� ������ ��������� ������� 2 �������";
                return;
            }

            // ��������� email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "������� email";
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "������� ���������� email �����";
                return;
            }

            // ��������� ������
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "������� ������";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "������ ������ ��������� ������� 6 ��������";
                return;
            }

            // �������� ������������� ������
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "������ �� ���������";
                return;
            }

            var success = await _authService.CreateUser(Email, Password, DisplayName);

            if (success)
            {
                SuccessMessage = "����������� ������ �������!";
            }
            else
            {
                ErrorMessage = "������ �����������. ��������, email ��� ������������";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = GetUserFriendlyErrorMessage(ex);
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

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private string GetUserFriendlyErrorMessage(Exception ex)
    {
        var errorMessage = ex.Message.ToLower();

        if (errorMessage.Contains("network") || errorMessage.Contains("internet"))
            return "��������� ����������� � ���������";

        if (errorMessage.Contains("email_already_in_use") || errorMessage.Contains("email already exists"))
            return "���� email ��� ������������";

        if (errorMessage.Contains("invalid_email") || errorMessage.Contains("invalid email"))
            return "�������� ������ email";

        if (errorMessage.Contains("user_not_found") || errorMessage.Contains("user not found"))
            return "������������ � ����� email �� ������";

        if (errorMessage.Contains("wrong_password") || errorMessage.Contains("invalid password"))
            return "�������� ������";

        if (errorMessage.Contains("weak_password") || errorMessage.Contains("password is weak"))
            return "������ ������� ������";

        return "��������� ������. ���������� ��� ���";
    }

    private void ValidatePassword()
    {
        if (string.IsNullOrEmpty(Password))
        {
            PasswordValidationMessage = "";
            return;
        }

        if (Password.Length < 6)
        {
            PasswordValidationMessage = "������� �������� ������";
        }
        else
        {
            PasswordValidationMessage = "������ ��������";
        }
    }

    private void ValidateConfirmPassword()
    {
        if (string.IsNullOrEmpty(ConfirmPassword))
        {
            ConfirmPasswordValidationMessage = "����������� ������";
            PasswordsMatch = false;
            return;
        }

        if (Password == ConfirmPassword)
        {
            ConfirmPasswordValidationMessage = "������ ���������";
            PasswordsMatch = true;
        }
        else
        {
            ConfirmPasswordValidationMessage = "������ �� ���������";
            PasswordsMatch = false;
        }
    }
}