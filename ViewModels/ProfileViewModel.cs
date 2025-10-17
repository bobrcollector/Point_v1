
using Point_v1.Models;
using Point_v1.Services;
using Point_v1.Views;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IAuthStateService _authStateService;
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    public ProfileViewModel(IAuthStateService authStateService, IDataService dataService, INavigationService navigationService)
    {
        _authStateService = authStateService;
        _dataService = dataService;
        _navigationService = navigationService;

        // Подписываемся на изменение состояния аутентификации
        _authStateService.AuthenticationStateChanged += OnAuthenticationStateChanged;

        // Инициализация команд
        EditProfileCommand = new Command(async () => await EditProfile());
        SaveProfileCommand = new Command(async () => await SaveProfile());
        CancelCommand = new Command(async () => await Cancel());
        SelectInterestsCommand = new Command(async () => await SelectInterests());
        SignOutCommand = new Command(async () => await SignOut());
        ToggleInterestCommand = new Command<Interest>((interest) => ToggleInterest(interest));
        SaveInterestsCommand = new Command(async () => await SaveInterests());
        GoToLoginCommand = new Command(async () => await GoToLogin());
        ChangeAvatarCommand = new Command(async () => await ChangeAvatar());

        // Загружаем данные пользователя
        _ = LoadUserData();
        LoadAvatar();
    }

    // Свойства для временных данных
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

    // Основные свойства профиля
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

    // Свойства для аватара
    private ImageSource _avatarImage = "👤";
    public ImageSource AvatarImage
    {
        get => _avatarImage;
        set => SetProperty(ref _avatarImage, value);
    }

    private string _avatarPath;
    public string AvatarPath
    {
        get => _avatarPath;
        set => SetProperty(ref _avatarPath, value);
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
    public ICommand ChangeAvatarCommand { get; }

    private void OnAuthenticationStateChanged(object sender, EventArgs e)
    {
        UpdateAuthState();
    }

    private void UpdateAuthState()
    {
        IsGuestMode = !_authStateService.IsAuthenticated;
        IsAuthenticated = _authStateService.IsAuthenticated;

        if (IsAuthenticated)
        {
            _ = LoadUserData();
        }
        else
        {
            // Сбрасываем данные при выходе
            UserName = "Пользователь";
            UserEmail = "";
            DisplayName = "";
            City = "";
            About = "";
            SelectedInterests = new List<Interest>();
        }
    }

    private async Task LoadUserData()
    {
        if (_authStateService.IsAuthenticated)
        {
            IsAuthenticated = true;
            IsGuestMode = false;

            var userId = _authStateService.CurrentUserId;
            System.Diagnostics.Debug.WriteLine($"🔄 Загрузка профиля для пользователя: {userId}");

            // Загружаем данные пользователя из базы
            var user = await _dataService.GetUserAsync(userId);
            if (user != null)
            {
                UserName = user.DisplayName;
                UserEmail = user.Email;
                DisplayName = user.DisplayName;
                City = user.City;
                About = user.About;

                // Загружаем интересы пользователя
                SelectedInterests = await GetUserInterests(user.InterestIds);
                System.Diagnostics.Debug.WriteLine($"👤 Загружено интересов пользователя: {SelectedInterests.Count}");
                System.Diagnostics.Debug.WriteLine($"👤 Данные профиля: {user.DisplayName}, {user.Email}, {user.City}");
            }
            else
            {
                // Если пользователь не найден в базе, используем базовые данные
                UserName = "Пользователь";
                UserEmail = userId;
                SelectedInterests = new List<Interest>();
                System.Diagnostics.Debug.WriteLine($"❌ Профиль не найден для пользователя: {userId}");
            }
        }
        else
        {
            IsAuthenticated = false;
            IsGuestMode = true;
            SelectedInterests = new List<Interest>();
            System.Diagnostics.Debug.WriteLine("🔐 Пользователь не авторизован");
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
            var userId = _authStateService.CurrentUserId;
            // Сохраняем данные пользователя
            var user = new User
            {
                Id = userId,
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
                // ОБНОВЛЯЕМ ДАННЫЕ НА СТРАНИЦЕ ПРОФИЛЯ
                UserName = DisplayName;

                // СОХРАНЯЕМ ПУТЬ К АВАТАРУ (если он был изменен)
                if (!string.IsNullOrEmpty(AvatarPath))
                {
                    Preferences.Set("UserAvatarPath", AvatarPath);
                }

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
        // Нужно использовать IAuthService для выхода
        var authService = Application.Current.Handler.MauiContext.Services.GetService<IAuthService>();
        await authService.SignOut();
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
            if (_authStateService.IsAuthenticated)
            {
                var user = await _dataService.GetUserAsync(_authStateService.CurrentUserId);
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
                Id = _authStateService.CurrentUserId,
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

    private async Task ChangeAvatar()
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var file = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Выберите аватар"
                });

                if (file != null)
                {
                    var userId = _authStateService.CurrentUserId;
                    // СОХРАНЯЕМ ФАЙЛ ЛОКАЛЬНО
                    var localFilePath = Path.Combine(FileSystem.CacheDirectory, $"avatar_{userId}.jpg");

                    using (var sourceStream = await file.OpenReadAsync())
                    using (var localStream = File.OpenWrite(localFilePath))
                    {
                        await sourceStream.CopyToAsync(localStream);
                    }

                    // Сохраняем путь к файлу
                    AvatarPath = localFilePath;
                    AvatarImage = ImageSource.FromFile(localFilePath);

                    // Сохраняем путь в настройках
                    Preferences.Set("UserAvatarPath", localFilePath);

                    System.Diagnostics.Debug.WriteLine($"📸 Аватар сохранен: {localFilePath}");
                    await Application.Current.MainPage.DisplayAlert("Успех", "Аватар изменен", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Функция выбора фото не поддерживается на этом устройстве", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка выбора аватара: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось выбрать фото", "OK");
        }
    }

    private void LoadAvatar()
    {
        try
        {
            var savedAvatarPath = Preferences.Get("UserAvatarPath", string.Empty);
            if (!string.IsNullOrEmpty(savedAvatarPath) && File.Exists(savedAvatarPath))
            {
                AvatarPath = savedAvatarPath;
                AvatarImage = ImageSource.FromFile(savedAvatarPath);
                System.Diagnostics.Debug.WriteLine($"📸 Аватар загружен: {savedAvatarPath}");
            }
            else
            {
                AvatarImage = "👤"; // Дефолтный аватар
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки аватара: {ex.Message}");
            AvatarImage = "👤";
        }
    }
}
