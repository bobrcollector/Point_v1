using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class SelectInterestsViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    public SelectInterestsViewModel(
        IAuthService authService,
        IDataService dataService,
        INavigationService navigationService,
        List<Interest> allInterests,
        List<Interest> selectedInterests)
    {
        _authService = authService;
        _dataService = dataService;
        _navigationService = navigationService;

        AllInterests = allInterests;
        SelectedInterests = new List<Interest>(selectedInterests);

        foreach (var interest in AllInterests)
        {
            interest.IsSelected = SelectedInterests.Any(si => si.Id == interest.Id);
        }

        ToggleInterestCommand = new Command<Interest>((interest) => ToggleInterest(interest));
        SaveInterestsCommand = new Command(async () => await SaveInterests());
        CancelCommand = new Command(async () => await Cancel());

        System.Diagnostics.Debug.WriteLine($"🎯 SelectInterestsViewModel создан: {AllInterests.Count} интересов, {SelectedInterests.Count} выбрано");
    }

    private List<Interest> _allInterests = new();
    public List<Interest> AllInterests
    {
        get => _allInterests;
        set => SetProperty(ref _allInterests, value);
    }

    private List<Interest> _selectedInterests = new();
    public List<Interest> SelectedInterests
    {
        get => _selectedInterests;
        set => SetProperty(ref _selectedInterests, value);
    }

    public ICommand ToggleInterestCommand { get; }
    public ICommand SaveInterestsCommand { get; }
    public ICommand CancelCommand { get; }

    private void ToggleInterest(Interest interest)
    {
        if (interest != null)
        {
            interest.IsSelected = !interest.IsSelected;
            SelectedInterests = AllInterests.Where(i => i.IsSelected).ToList();

            System.Diagnostics.Debug.WriteLine($"🎯 Интерес '{interest.Name}' {(interest.IsSelected ? "выбран" : "удален")}");
            System.Diagnostics.Debug.WriteLine($"📊 Теперь выбрано: {SelectedInterests.Count} интересов");
        }
    }

    private async Task SaveInterests()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"💾 Сохранение {SelectedInterests.Count} интересов...");
            var user = new User
            {
                Id = _authService.CurrentUserId,
                InterestIds = SelectedInterests.Select(i => i.Id).ToList(),
                UpdatedAt = DateTime.UtcNow
            };

            var success = await _dataService.UpdateUserAsync(user);

            if (success)
            {
                await _navigationService.GoToAsync("..");
                await Application.Current.MainPage.DisplayAlert("Успех", "Интересы сохранены", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сохранить интересы", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка сохранения интересов: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async Task Cancel()
    {
        await _navigationService.GoToAsync("..");
    }
}