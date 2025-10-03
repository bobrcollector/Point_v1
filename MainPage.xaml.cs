namespace Point_v1;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnLoginBtnClicked(object sender, EventArgs e)
    {
        // Используем Shell навигацию вместо NavigationPage
        await Shell.Current.GoToAsync("//LoginPage");
    }
}