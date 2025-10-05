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
        DisplayAlert("Восстановление пароля", "Функция восстановления пароля будет доступна в ближайшее время", "OK");
    }

    private void OnGoogleLoginClicked(object sender, EventArgs e)
    {
        DisplayAlert("Google вход", "Вход через Google будет доступен в ближайшее время", "OK");
    }

    private void OnVkLoginClicked(object sender, EventArgs e)
    {
        DisplayAlert("VK вход", "Вход через VK будет доступен в ближайшее время", "OK");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//RegisterPage");
    }

}