using Point_v1.Views;

namespace Point_v1.Services;

public interface INavigationService
{
    Task GoToEventDetailsAsync(string eventId);
    Task GoToLoginAsync();
    Task GoBackAsync();
    Task GoToHomeAsync();
}

public class NavigationService : INavigationService
{
    public async Task GoToEventDetailsAsync(string eventId)
    {
        await Shell.Current.GoToAsync($"{nameof(EventDetailsPage)}?eventId={eventId}");
    }

    public async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    public async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    public async Task GoToHomeAsync()
    {
        await Shell.Current.GoToAsync("//HomePage");
    }
}