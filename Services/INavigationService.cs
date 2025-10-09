namespace Point_v1.Services;

public interface INavigationService
{
    Task GoToHomeAsync();
    Task GoToLoginAsync();
    Task GoToProfileAsync();
}