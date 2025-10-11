using Point_v1.Views;

namespace Point_v1.Services;

public class NavigationService : INavigationService
{
    public async Task GoToHomeAsync()
    {
        await Shell.Current.GoToAsync("//HomePage");
    }

    public async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    public async Task GoToProfileAsync()
    {
        await Shell.Current.GoToAsync("//ProfilePage");
    }

    // днаюбэ щрнр лернд
    public async Task GoToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }
}