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

    // ДОБАВЬ ЭТИ СВОЙСТВА В КЛАСС ProfileViewModel:

    private List<Interest> _tempSelectedInterests = new();
    public List<Interest> TempSelectedInterests
    {
        get => _tempSelectedInterests;
        set => SetProperty(ref _tempSelectedInterests, value);
    }

    private List<Interest> _tempAllInterests = new();
    public List<Interest> TempAllInterests
    {
        get => _tempAllInterests;
        set => SetProperty(ref _tempAllInterests, value);
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

                // ВАЖНО: Загружаем интересы пользователя и сохраняем в SelectedInterests
                SelectedInterests = await GetUserInterests(user.InterestIds);
                System.Diagnostics.Debug.WriteLine($"👤 Загружено интересов пользователя: {SelectedInterests.Count}");
            }
            else
            {
                // Если пользователь не найден в базе, используем базовые данные
                UserName = "Пользователь";
                UserEmail = _authService.CurrentUserId;
                SelectedInterests = new List<Interest>(); // Инициализируем пустым списком
            }
        }
        else
        {
            IsAuthenticated = false;
            IsGuestMode = true;
            SelectedInterests = new List<Interest>(); // Инициализируем пустым списком
        }
    }

    private async Task<List<Interest>> GetUserInterests(List<string> interestIds)
    {
        if (interestIds == null || interestIds.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("👤 У пользователя нет выбранных интересов");
            return new List<Interest>();
        }

        var allInterests = await _dataService.GetInterestsAsync();
        var userInterests = allInterests.Where(i => interestIds.Contains(i.Id)).ToList();

        System.Diagnostics.Debug.WriteLine($"👤 Найдено интересов пользователя: {userInterests.Count} из {interestIds.Count} ID");

        return userInterests;
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
        // ПРОСТО ВОЗВРАЩАЕМСЯ БЕЗ СОХРАНЕНИЯ
        // Временные данные будут отброшены
        System.Diagnostics.Debug.WriteLine("❌ Отмена выбора интересов - изменения не сохранены");
        await _navigationService.GoToAsync($"../{nameof(EditProfilePage)}");
    }

    private async Task SelectInterests()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 Начало загрузки интересов...");

            // ЗАГРУЖАЕМ ДАННЫЕ ПЕРЕД ПЕРЕХОДОМ
            await LoadAllInterests();

            // КОПИРУЕМ ДАННЫЕ ВО ВРЕМЕННЫЕ СПИСКИ
            CopyToTempData();

            System.Diagnostics.Debug.WriteLine($"📊 Загружено интересов во временном списке: {TempAllInterests?.Count ?? 0}");

            if (TempAllInterests?.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("✅ Интересы загружены, переходим на страницу выбора");
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

    // Публичный метод для загрузки интересов (из страницы)
    public async Task LoadInterestsForSelection()
    {
        await LoadAllInterests();
    }

    private async Task LoadAllInterests()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 Загрузка интересов из DataService...");

            // ОТЛАДКА: проверяем текущее состояние
            System.Diagnostics.Debug.WriteLine($"📊 Текущее состояние - SelectedInterests: {SelectedInterests?.Count ?? 0}");

            var interests = await _dataService.GetInterestsAsync();

            if (interests == null || interests.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("❌ Не удалось загрузить интересы из базы");
                return;
            }

            // ВАЖНО: ЕСЛИ SelectedInterests ПУСТОЙ - ЗАГРУЖАЕМ ИЗ БАЗЫ
            if (SelectedInterests?.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("🔄 SelectedInterests пустой, загружаем из базы данных пользователя...");
                await LoadUserInterestsFromDatabase();
            }

            // СОХРАНЯЕМ ВЫБРАННЫЕ ID ИЗ ОСНОВНОГО СПИСКА
            var selectedIds = SelectedInterests?.Select(si => si.Id).ToList() ?? new List<string>();

            System.Diagnostics.Debug.WriteLine($"🎯 Сохранено выбранных ID: {selectedIds.Count}");

            // ИСПОЛЬЗУЕМ ВРЕМЕННЫЙ СПИСОК ДЛЯ ВЫБОРА
            TempAllInterests = interests;
            System.Diagnostics.Debug.WriteLine($"📥 Получено интересов из базы: {interests.Count}");

            // Помечаем выбранные интересы ВО ВРЕМЕННОМ СПИСКЕ
            foreach (var interest in TempAllInterests)
            {
                interest.IsSelected = selectedIds.Contains(interest.Id);
                System.Diagnostics.Debug.WriteLine($"🎯 Интерес '{interest.Name}': {interest.IsSelected} (ID: {interest.Id})");
            }

            // ОБНОВЛЯЕМ ВРЕМЕННЫЙ SelectedInterests
            TempSelectedInterests = TempAllInterests.Where(i => i.IsSelected).ToList();

            System.Diagnostics.Debug.WriteLine($"🎯 Итоговое количество выбранных во временном списке: {TempSelectedInterests.Count}");

            // ПРИНУДИТЕЛЬНО ОБНОВЛЯЕМ ПРИВЯЗКУ ДЛЯ ВРЕМЕННЫХ ДАННЫХ
            OnPropertyChanged(nameof(TempAllInterests));
            OnPropertyChanged(nameof(TempSelectedInterests));

            System.Diagnostics.Debug.WriteLine("✅ Временные привязки обновлены");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки интересов: {ex.Message}");
        }
    }

    // Новый метод для загрузки интересов пользователя из базы
    private async Task LoadUserInterestsFromDatabase()
    {
        try
        {
            if (_authService.IsAuthenticated)
            {
                var user = await _dataService.GetUserAsync(_authService.CurrentUserId);
                if (user != null && user.InterestIds?.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"👤 Загружаем интересы пользователя из базы: {user.InterestIds.Count} ID");
                    SelectedInterests = await GetUserInterests(user.InterestIds);
                    System.Diagnostics.Debug.WriteLine($"👤 Загружено интересов: {SelectedInterests.Count}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки интересов пользователя: {ex.Message}");
        }
    }

    private void ToggleInterest(Interest interest)
    {
        if (interest != null)
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Переключаем интерес: {interest.Name} -> {!interest.IsSelected}");

            // ПЕРЕКЛЮЧАЕМ ВЫБОР
            interest.IsSelected = !interest.IsSelected;

            // ОБНОВЛЯЕМ ВРЕМЕННЫЕ ВЫБРАННЫЕ ИНТЕРЕСЫ
            TempSelectedInterests = TempAllInterests.Where(i => i.IsSelected).ToList();

            System.Diagnostics.Debug.WriteLine($"📊 Теперь выбрано во временном списке: {TempSelectedInterests.Count} интересов");

            // ВАЖНО: ПРИНУДИТЕЛЬНОЕ ОБНОВЛЕНИЕ ВСЕГО СПИСКА
            // Создаем новый список, чтобы заставить FlexLayout перерисоваться
            var newList = new List<Interest>(TempAllInterests);
            TempAllInterests = null;
            TempAllInterests = newList;

            // ОБНОВЛЯЕМ ПРИВЯЗКИ
            OnPropertyChanged(nameof(TempAllInterests));
            OnPropertyChanged(nameof(TempSelectedInterests));

            System.Diagnostics.Debug.WriteLine("🔄 Привязки обновлены после переключения");
        }
    }


    private async Task SaveInterests()
    {
        try
        {
            // ПЕРЕНОСИМ ДАННЫЕ ИЗ ВРЕМЕННОГО СПИСКА В ОСНОВНОЙ
            SelectedInterests = new List<Interest>(TempSelectedInterests);

            System.Diagnostics.Debug.WriteLine($"💾 Сохранение {SelectedInterests.Count} интересов...");

            // Сохраняем ВЕСЬ профиль с обновленными интересами
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

            // ОТЛАДКА: выводим какие интересы сохраняются
            System.Diagnostics.Debug.WriteLine("💾 Сохраняемые интересы:");
            foreach (var interest in SelectedInterests)
            {
                System.Diagnostics.Debug.WriteLine($"💾 - {interest.Name} (ID: {interest.Id})");
            }

            var success = await _dataService.UpdateUserAsync(user);

            if (success)
            {
                System.Diagnostics.Debug.WriteLine("✅ Интересы успешно сохранены в базу");

                // Возвращаемся в редактирование профиля
                await _navigationService.GoToAsync($"../{nameof(EditProfilePage)}");
                await Application.Current.MainPage.DisplayAlert("Успех", "Интересы сохранены", "OK");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Не удалось сохранить интересы в базу");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сохранить интересы", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка сохранения интересов: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    public void CopyToTempData()
    {
        System.Diagnostics.Debug.WriteLine($"📋 Копируем данные в временные списки: {SelectedInterests?.Count ?? 0} выбранных интересов");

        // Копируем основные данные во временные
        if (AllInterests?.Count > 0)
        {
            TempAllInterests = new List<Interest>(AllInterests);
            TempSelectedInterests = new List<Interest>(SelectedInterests ?? new List<Interest>());

            System.Diagnostics.Debug.WriteLine($"📋 Скопировано: {TempAllInterests.Count} интересов, {TempSelectedInterests.Count} выбранных");

            // ОБНОВЛЯЕМ ПРИВЯЗКИ
            OnPropertyChanged(nameof(TempAllInterests));
            OnPropertyChanged(nameof(TempSelectedInterests));
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("📋 Нет данных для копирования");
        }
    }

    public void PrepareForInterestSelection()
    {
        System.Diagnostics.Debug.WriteLine($"🎯 PrepareForInterestSelection - SelectedInterests: {SelectedInterests?.Count ?? 0}, TempAllInterests: {TempAllInterests?.Count ?? 0}");
    }
}