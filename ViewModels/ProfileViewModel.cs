using Point_v1.Models;
using Point_v1.Services;
using Point_v1.Views;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    public ProfileViewModel(IAuthService authService, IDataService dataService, INavigationService navigationService)
    {
        _authService = authService;
        _dataService = dataService;
        _navigationService = navigationService;

        // Инициализация команд
        EditProfileCommand = new Command(async () => await EditProfile());
        SaveProfileCommand = new Command(async () => await SaveProfile());
        CancelCommand = new Command(async () => await Cancel());
        SelectInterestsCommand = new Command(async () => await SelectInterests());
        SignOutCommand = new Command(async () => await SignOut());
        ToggleInterestCommand = new Command<Interest>((interest) => ToggleInterest(interest));
        SaveInterestsCommand = new Command(async () => await SaveInterests());
        GoToLoginCommand = new Command(async () => await GoToLogin());

        // Загружаем данные пользователя
        _ = LoadUserData();
    }

    private string _userName = "Пользователь";
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private string _userEmail = "";
    public string UserEmail
    {
        get => _userEmail;
        set => SetProperty(ref _userEmail, value);
    }

    private string _displayName = "";
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    private string _city = "";
    public string City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    private string _about = "";
    public string About
    {
        get => _about;
        set => SetProperty(ref _about, value);
    }

    private List<Interest> _selectedInterests = new();
    public List<Interest> SelectedInterests
    {
        get => _selectedInterests;
        set => SetProperty(ref _selectedInterests, value);
    }

    private List<Interest> _allInterests = new();
    public List<Interest> AllInterests
    {
        get => _allInterests;
        set => SetProperty(ref _allInterests, value);
    }

    private bool _isAuthenticated;
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => SetProperty(ref _isAuthenticated, value);
    }

    private bool _isGuestMode = true;
    public bool IsGuestMode
    {
        get => _isGuestMode;
        set => SetProperty(ref _isGuestMode, value);
    }

    // Команды
    public ICommand EditProfileCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SelectInterestsCommand { get; }
    public ICommand SignOutCommand { get; }
    public ICommand GoToLoginCommand { get; }
    public ICommand ToggleInterestCommand { get; }
    public ICommand SaveInterestsCommand { get; }

    private async Task LoadUserData()
    {
        if (_authService.IsAuthenticated)
        {
            IsAuthenticated = true;
            IsGuestMode = false;

            // Загружаем данные пользователя из базы
            var user = await _dataService.GetUserAsync(_authService.CurrentUserId);
            if (user != null)
            {
                UserName = user.DisplayName;
                UserEmail = user.Email;
                DisplayName = user.DisplayName;
                City = user.City;
                About = user.About;
                SelectedInterests = await GetUserInterests(user.InterestIds);
            }
            else
            {
                // Если пользователь не найден в базе, используем базовые данные
                UserName = "Пользователь";
                UserEmail = _authService.CurrentUserId;
            }
        }
        else
        {
            IsAuthenticated = false;
            IsGuestMode = true;
        }
    }

    private async Task<List<Interest>> GetUserInterests(List<string> interestIds)
    {
        var allInterests = await _dataService.GetInterestsAsync();
        return allInterests.Where(i => interestIds.Contains(i.Id)).ToList();
    }

    private async Task EditProfile()
    {
        await _navigationService.GoToAsync(nameof(EditProfilePage));
    }

    private async Task SaveProfile()
    {
        try
        {
            // Сохраняем данные пользователя
            var user = new User
            {
                Id = _authService.CurrentUserId,
                DisplayName = DisplayName,
                Email = UserEmail,
                City = City,
                About = About,
                InterestIds = SelectedInterests.Select(i => i.Id).ToList(),
                UpdatedAt = DateTime.UtcNow
            };

            var success = await _dataService.UpdateUserAsync(user);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Успех", "Профиль сохранен", "OK");
                await _navigationService.GoToProfileAsync();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сохранить профиль", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async Task Cancel()
    {
        await _navigationService.GoToProfileAsync();
    }

    private async Task SelectInterests()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 Начало загрузки интересов...");

            await LoadAllInterests();

            System.Diagnostics.Debug.WriteLine($"📊 Загружено интересов: {AllInterests?.Count ?? 0}");

            if (AllInterests?.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("✅ Интересы загружены, переходим на страницу выбора");

                // ДОБАВИМ ЗАДЕРЖКУ И ПРИНУДИТЕЛЬНОЕ ОБНОВЛЕНИЕ
                await Task.Delay(100);
                OnPropertyChanged(nameof(AllInterests));
                OnPropertyChanged(nameof(SelectedInterests));

                await _navigationService.GoToAsync(nameof(SelectInterestsPage));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Интересы не загружены!");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось загрузить список интересов", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в SelectInterests: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async Task SignOut()
    {
        await _authService.SignOut();
        await _navigationService.GoToLoginAsync();
    }

    private async Task GoToLogin()
    {
        await _navigationService.GoToLoginAsync();
    }

    private async Task LoadAllInterests()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 Загрузка интересов из DataService...");

            var interests = await _dataService.GetInterestsAsync();
            AllInterests = interests;

            System.Diagnostics.Debug.WriteLine($"📥 Получено интересов: {interests?.Count ?? 0}");

            // Помечаем выбранные интересы
            foreach (var interest in AllInterests)
            {
                interest.IsSelected = SelectedInterests.Any(si => si.Id == interest.Id);
            }

            System.Diagnostics.Debug.WriteLine($"🎯 Помечено выбранных: {AllInterests.Count(i => i.IsSelected)}");

            // Принудительно обновляем привязку
            OnPropertyChanged(nameof(AllInterests));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки интересов: {ex.Message}");
        }
    }

    private void ToggleInterest(Interest interest)
    {
        if (interest != null)
        {
            interest.IsSelected = !interest.IsSelected;

            // Обновляем выбранные интересы
            SelectedInterests = AllInterests.Where(i => i.IsSelected).ToList();

            System.Diagnostics.Debug.WriteLine($"🎯 Интерес '{interest.Name}' {(interest.IsSelected ? "выбран" : "удален")}");
        }
    }

    private async Task SaveInterests()
    {
        try
        {
            // Обновляем выбранные интересы
            SelectedInterests = AllInterests.Where(i => i.IsSelected).ToList();

            // Сохраняем профиль
            await SaveProfile();
            await _navigationService.GoToAsync("..");

            await Application.Current.MainPage.DisplayAlert("Успех", "Интересы сохранены", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}