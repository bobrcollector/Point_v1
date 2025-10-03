using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnEditProfileClicked(object sender, EventArgs e)
    {
        DisplayAlert("Редактирование", "Функция редактирования профиля будет доступна скоро", "OK");
    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        DisplayAlert("Настройки", "Настройки приложения будут доступны скоро", "OK");
    }
}