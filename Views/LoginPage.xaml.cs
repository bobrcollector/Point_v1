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

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//RegisterPage");
    }

}