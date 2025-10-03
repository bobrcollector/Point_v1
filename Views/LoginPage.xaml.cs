using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        DisplayAlert("�������������� ������", "������� �������������� ������ ����� �������� � ��������� �����", "OK");
    }

    private void OnGoogleLoginClicked(object sender, EventArgs e)
    {
        DisplayAlert("Google ����", "���� ����� Google ����� �������� � ��������� �����", "OK");
    }

    private void OnVkLoginClicked(object sender, EventArgs e)
    {
        DisplayAlert("VK ����", "���� ����� VK ����� �������� � ��������� �����", "OK");
    }

    private void OnRegisterTapped(object sender, EventArgs e)
    {
        if (BindingContext is AuthViewModel viewModel)
        {
            viewModel.IsRegisterMode = true;
        }
    }
}