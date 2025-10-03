using Point_v1.Models;
using Point_v1.Services;
using Point_v1.Views;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly IAuthStateService _authStateService;
    private readonly INavigationService _navigationService;

    public HomeViewModel(IAuthStateService authStateService, INavigationService navigationService)
    {
        _authStateService = authStateService;
        _navigationService = navigationService;

        GoToLoginCommand = new Command(async () => await GoToLogin());
        CreateEventCommand = new Command(async () => await CreateEvent());
        JoinEventCommand = new Command<string>(async (eventId) => await JoinEvent(eventId));
        OpenFiltersCommand = new Command(async () => await OpenFilters());

        UpdateAuthState();
    }

    private bool _isGuestMode = true;
    public bool IsGuestMode
    {
        get => _isGuestMode;
        set => SetProperty(ref _isGuestMode, value);
    }

    public ICommand GoToLoginCommand { get; }
    public ICommand CreateEventCommand { get; }
    public ICommand JoinEventCommand { get; }
    public ICommand OpenFiltersCommand { get; }

    private void UpdateAuthState()
    {
        IsGuestMode = !_authStateService.IsAuthenticated;
    }

    private async Task GoToLogin()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Кнопка Войти нажата!");
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка навигации: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось перейти на страницу входа", "OK");
        }
    }

    private async Task CreateEvent()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Кнопка Создать событие нажата!");
            if (IsGuestMode)
            {
                await GoToLogin();
                return;
            }
            // Используем правильный синтаксис для Shell навигации
            await Shell.Current.GoToAsync("//CreateEventPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка навигации: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось перейти к созданию события", "OK");
        }
    }
    private async Task JoinEvent(string eventId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Кнопка Я пойду! нажата для события {eventId}");
            if (IsGuestMode)
            {
                await GoToLogin();
                return;
            }

            // Временная логика - показываем сообщение
            await Application.Current.MainPage.DisplayAlert("Успех!", $"Вы присоединились к событию {eventId}", "OK");

            // Здесь можно добавить реальную логику участия
            // await _dataService.JoinEventAsync(eventId, _authStateService.CurrentUserId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка участия в событии: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось присоединиться к событию", "OK");
        }
    }

    private async Task OpenFilters()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Кнопка Фильтры нажата!");
            // Используем правильный синтаксис для Shell навигации
            await Shell.Current.GoToAsync("//FilterPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка навигации: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось открыть фильтры", "OK");
        }
    }

}