
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
    private readonly FirebaseRestService _firebaseRest;

    public ProfileViewModel(IAuthStateService authStateService, IDataService dataService, INavigationService navigationService)
    {
        _authStateService = authStateService;
        _dataService = dataService;
        _navigationService = navigationService;
        _firebaseRest = new FirebaseRestService();

        _authStateService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        EditProfileCommand = new Command(async () => await EditProfile());
        SaveProfileCommand = new Command(async () => await SaveProfile());
        CancelCommand = new Command(async () => await Cancel());
        SelectInterestsCommand = new Command(async () => await SelectInterests());
        SignOutCommand = new Command(async () => await SignOut());
        ToggleInterestCommand = new Command<Interest>((interest) => ToggleInterestDirect(interest));
        SaveInterestsCommand = new Command(async () => await SaveInterests());
        GoToLoginCommand = new Command(async () => await GoToLogin());
        ChangeAvatarCommand = new Command(async () => await ChangeAvatar());
        GoToSettingsCommand = new Command(async () => await GoToSettings());

        _ = LoadUserData();
    }


    private int _createdEventsCount;
    public int CreatedEventsCount
    {
        get => _createdEventsCount;
        set => SetProperty(ref _createdEventsCount, value);
    }

    private int _participatedEventsCount;
    public int ParticipatedEventsCount
    {
        get => _participatedEventsCount;
        set => SetProperty(ref _participatedEventsCount, value);
    }

    private int _upcomingEventsCount;
    public int UpcomingEventsCount
    {
        get => _upcomingEventsCount;
        set => SetProperty(ref _upcomingEventsCount, value);
    }

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
        set
        {
            SetProperty(ref _selectedInterests, value);
            UpdateInterestsStatus();
        }
    }
    
    private List<Interest> _allInterests = new();
    public List<Interest> AllInterests
    {
        get => _allInterests;
        set => SetProperty(ref _allInterests, value);
    }
    
    private string _interestsStatus = "";
    public string InterestsStatus
    {
        get => _interestsStatus;
        set => SetProperty(ref _interestsStatus, value);
    }
    
    public ICommand ToggleInterestCommand { get; }

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

    public ICommand EditProfileCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SelectInterestsCommand { get; }
    public ICommand SignOutCommand { get; }
    public ICommand GoToLoginCommand { get; }
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
            UserName = "Пользователь";
            UserEmail = "";
            DisplayName = "";
            City = "";
            About = "";
            SelectedInterests = new List<Interest>();

            CreatedEventsCount = 0;
            ParticipatedEventsCount = 0;
            UpcomingEventsCount = 0;
        }
    }

    public async Task LoadUserData()
    {
        if (_authStateService.IsAuthenticated)
        {
            IsAuthenticated = true;
            IsGuestMode = false;

            var userId = _authStateService.CurrentUserId;
            System.Diagnostics.Debug.WriteLine($"🔄 Загрузка профиля для пользователя: {userId}");

            var user = await _dataService.GetUserAsync(userId);
            if (user != null)
            {
                UserName = user.DisplayName;
                UserEmail = user.Email;
                DisplayName = user.DisplayName;
                City = user.City;
                About = user.About;

                SelectedInterests = await GetUserInterests(user.InterestIds);
                System.Diagnostics.Debug.WriteLine($"👤 Загружено интересов пользователя: {SelectedInterests.Count}");

                await LoadUserStatistics(userId);
                await LoadAvatarAsync();
            }
            else
            {
                UserName = "Пользователь";
                UserEmail = userId;
                SelectedInterests = new List<Interest>();

                CreatedEventsCount = 0;
                ParticipatedEventsCount = 0;
                UpcomingEventsCount = 0;

                System.Diagnostics.Debug.WriteLine($"❌ Профиль не найден для пользователя: {userId}");
            }
        }
        else
        {
            IsAuthenticated = false;
            IsGuestMode = true;
            SelectedInterests = new List<Interest>();

            CreatedEventsCount = 0;
            ParticipatedEventsCount = 0;
            UpcomingEventsCount = 0;

            System.Diagnostics.Debug.WriteLine("🔐 Пользователь не авторизован");
        }
    }


    public async Task LoadUserStatistics(string userId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📊 Загрузка статистики для пользователя: {userId}");

            var createdTask = _dataService.GetUserCreatedEventsCountAsync(userId);
            var participatedTask = _dataService.GetUserParticipatedEventsCountAsync(userId);
            var upcomingTask = _dataService.GetUserUpcomingEventsCountAsync(userId);

            await Task.WhenAll(createdTask, participatedTask, upcomingTask);

            CreatedEventsCount = createdTask.Result;
            ParticipatedEventsCount = participatedTask.Result;
            UpcomingEventsCount = upcomingTask.Result;

            System.Diagnostics.Debug.WriteLine($"📊 Статистика загружена: " +
                $"Создано: {CreatedEventsCount}, " +
                $"Участвовал: {ParticipatedEventsCount}, " +
                $"Предстоящие: {UpcomingEventsCount}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки статистики: {ex.Message}");
            CreatedEventsCount = 0;
            ParticipatedEventsCount = 0;
            UpcomingEventsCount = 0;
        }
    }

    public string GetCurrentUserId()
    {
        return _authStateService.CurrentUserId;
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
            var currentUser = await _dataService.GetUserAsync(userId);
            var user = new User
            {
                Id = userId,
                DisplayName = DisplayName,
                Email = UserEmail,
                City = City,
                About = About,
                InterestIds = SelectedInterests.Select(i => i.Id).ToList(),
                AvatarUrl = currentUser?.AvatarUrl ?? string.Empty,
                UpdatedAt = DateTime.UtcNow
            };

            var success = await _dataService.UpdateUserAsync(user);

            if (success)
            {
                UserName = DisplayName;
                await LoadAvatarAsync();
                await LoadUserInterestsFromDatabase();
                OnPropertyChanged(nameof(SelectedInterests));

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

            await LoadAllInterests();
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
        var authService = Application.Current.Handler.MauiContext.Services.GetService<IAuthService>();
        await authService.SignOut();
        await _navigationService.GoToLoginAsync();
    }

    private async Task GoToLogin()
    {
        await _navigationService.GoToLoginAsync();
    }

    public async Task LoadInterestsForSelection()
    {
        await LoadAllInterests();
    }

    private async Task LoadAllInterests()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 Загрузка интересов из DataService...");

            System.Diagnostics.Debug.WriteLine($"📊 Текущее состояние - SelectedInterests: {SelectedInterests?.Count ?? 0}");
            var interests = await _dataService.GetInterestsAsync();
            if (interests == null || interests.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("❌ Не удалось загрузить интересы из базы");
                return;
            }
            if (SelectedInterests?.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("🔄 SelectedInterests пустой, загружаем из базы данных пользователя...");
                await LoadUserInterestsFromDatabase();
            }

            var selectedIds = SelectedInterests?.Select(si => si.Id).ToList() ?? new List<string>();
            System.Diagnostics.Debug.WriteLine($"🎯 Сохранено выбранных ID: {selectedIds.Count}");
            TempAllInterests = interests;
            System.Diagnostics.Debug.WriteLine($"📥 Получено интересов из базы: {interests.Count}");

            foreach (var interest in TempAllInterests)
            {
                interest.IsSelected = selectedIds.Contains(interest.Id);
                System.Diagnostics.Debug.WriteLine($"🎯 Интерес '{interest.Name}': {interest.IsSelected} (ID: {interest.Id})");
            }

            TempSelectedInterests = TempAllInterests.Where(i => i.IsSelected).ToList();
            System.Diagnostics.Debug.WriteLine($"🎯 Итоговое количество выбранных во временном списке: {TempSelectedInterests.Count}");
            OnPropertyChanged(nameof(TempAllInterests));
            OnPropertyChanged(nameof(TempSelectedInterests));

            System.Diagnostics.Debug.WriteLine("✅ Временные привязки обновлены");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки интересов: {ex.Message}");
        }
    }
    
    public async Task LoadAllInterestsForEdit()
    {
        try
        {
            if (SelectedInterests == null || SelectedInterests.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("🔄 SelectedInterests пустой, загружаем из базы...");
                await LoadUserInterestsFromDatabase();
            }
            
            var interests = await _dataService.GetInterestsAsync();

            if (interests == null || interests.Count == 0)
            {
                interests = new List<Interest>
                {
                    new Interest { Id = "1", Name = "🎲 Настольные игры", IsSelected = false },
                    new Interest { Id = "2", Name = "🎭 Косплей", IsSelected = false },
                    new Interest { Id = "3", Name = "🎨 Искусство", IsSelected = false },
                    new Interest { Id = "4", Name = "💻 Программирование", IsSelected = false },
                    new Interest { Id = "5", Name = "📺 Аниме", IsSelected = false },
                    new Interest { Id = "6", Name = "🎵 Музыка", IsSelected = false },
                    new Interest { Id = "7", Name = "🍳 Кулинария", IsSelected = false },
                    new Interest { Id = "8", Name = "📚 Книги", IsSelected = false },
                    new Interest { Id = "9", Name = "🚶‍♂️ Прогулки", IsSelected = false },
                    new Interest { Id = "10", Name = "🎬 Кино", IsSelected = false },
                    new Interest { Id = "16", Name = "🏥 Медицина", IsSelected = false },
                    new Interest { Id = "17", Name = "📌 Прочее", IsSelected = false }
                };
            }
            else
            {
                foreach (var interest in interests)
                {
                    interest.IsSelected = false;
                }
            }

            if (SelectedInterests != null && SelectedInterests.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"🎯 Восстанавливаем {SelectedInterests.Count} выбранных интересов");
                foreach (var selected in SelectedInterests)
                {
                    var interest = interests.FirstOrDefault(i => 
                        (i.Id == selected.Id && !string.IsNullOrEmpty(i.Id)) || 
                        i.Name == selected.Name);
                    if (interest != null)
                    {
                        interest.IsSelected = true;
                        System.Diagnostics.Debug.WriteLine($"✅ Интерес '{interest.Name}' помечен как выбранный");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Интерес '{selected.Name}' не найден в списке доступных");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ SelectedInterests пустой, нет интересов для восстановления");
            }

            AllInterests = interests;
            UpdateInterestsStatus();
            
            System.Diagnostics.Debug.WriteLine($"📋 Загружено {AllInterests.Count} интересов, выбрано {AllInterests.Count(i => i.IsSelected)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки интересов для редактирования: {ex.Message}");
        }
    }

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

            interest.IsSelected = !interest.IsSelected;
            TempSelectedInterests = TempAllInterests.Where(i => i.IsSelected).ToList();
            System.Diagnostics.Debug.WriteLine($"📊 Теперь выбрано во временном списке: {TempSelectedInterests.Count} интересов");
            var newList = new List<Interest>(TempAllInterests);
            TempAllInterests = null;
            TempAllInterests = newList;
            OnPropertyChanged(nameof(TempAllInterests));
            OnPropertyChanged(nameof(TempSelectedInterests));

            System.Diagnostics.Debug.WriteLine("🔄 Привязки обновлены после переключения");
        }
    }
    
    private void ToggleInterestDirect(Interest interest)
    {
        if (interest == null) return;

        if (interest.IsSelected)
        {
            SelectedInterests.RemoveAll(i => (i.Id == interest.Id && !string.IsNullOrEmpty(i.Id)) || i.Name == interest.Name);
            interest.IsSelected = false;
        }
        else
        {
            SelectedInterests.Add(interest);
            interest.IsSelected = true;
        }

        UpdateInterestsStatus();
        
        var updatedList = AllInterests?.ToList() ?? new List<Interest>();
        AllInterests = null;
        AllInterests = updatedList;
    }
    
    private void UpdateInterestsStatus()
    {
        var count = SelectedInterests.Count;
        if (count == 0)
        {
            InterestsStatus = "Выберите ваши интересы";
        }
        else
        {
            InterestsStatus = $"Выбрано интересов: {count}";
        }
    }

    private async Task SaveInterests()
    {
        try
        {
            SelectedInterests = new List<Interest>(TempSelectedInterests);
            System.Diagnostics.Debug.WriteLine($"💾 Сохранение {SelectedInterests.Count} интересов...");
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

            System.Diagnostics.Debug.WriteLine("💾 Сохраняемые интересы:");
            foreach (var interest in SelectedInterests)
            {
                System.Diagnostics.Debug.WriteLine($"💾 - {interest.Name} (ID: {interest.Id})");
            }

            var success = await _dataService.UpdateUserAsync(user);

            if (success)
            {
                System.Diagnostics.Debug.WriteLine("✅ Интересы успешно сохранены в базу");

                await LoadUserInterestsFromDatabase();
                OnPropertyChanged(nameof(SelectedInterests));
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

        if (AllInterests?.Count > 0)
        {
            TempAllInterests = new List<Interest>(AllInterests);
            TempSelectedInterests = new List<Interest>(SelectedInterests ?? new List<Interest>());

            System.Diagnostics.Debug.WriteLine($"📋 Скопировано: {TempAllInterests.Count} интересов, {TempSelectedInterests.Count} выбранных");

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

    public ICommand GoToSettingsCommand { get; }

    private async Task GoToSettings()
    {
        await _navigationService.GoToAsync(nameof(SettingsPage));
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
                    if (string.IsNullOrEmpty(userId))
                    {
                        await Application.Current.MainPage.DisplayAlert("Ошибка", "Необходима авторизация", "OK");
                        return;
                    }

                    var avatarsDirectory = Path.Combine(FileSystem.AppDataDirectory, "Avatars");
                    if (!Directory.Exists(avatarsDirectory))
                    {
                        Directory.CreateDirectory(avatarsDirectory);
                    }
                    
                    var localFilePath = Path.Combine(avatarsDirectory, $"avatar_{userId}.jpg");

                    using (var sourceStream = await file.OpenReadAsync())
                    using (var localStream = File.OpenWrite(localFilePath))
                    {
                        await sourceStream.CopyToAsync(localStream);
                    }

                    System.Diagnostics.Debug.WriteLine($"💾 Аватар сохранен локально: {localFilePath}");

                    AvatarPath = localFilePath;
                    AvatarImage = ImageSource.FromFile(localFilePath);

                    var idToken = await SecureStorage.GetAsync("firebase_token");
                    string avatarUrl = null;
                    
                    if (!string.IsNullOrEmpty(idToken))
                    {
                        try
                        {
                            avatarUrl = await _firebaseRest.UploadAvatarAsync(localFilePath, userId, idToken);
                            if (!string.IsNullOrEmpty(avatarUrl))
                            {
                                System.Diagnostics.Debug.WriteLine($"📸 Аватар загружен в Firebase Storage: {avatarUrl}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"⚠️ Не удалось загрузить в Firebase, используем локальный файл");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Ошибка загрузки в Firebase: {ex.Message}, используем локальный файл");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Токен не найден, используем только локальный файл");
                    }

                    var user = await _dataService.GetUserAsync(userId);
                    if (user != null)
                    {
                        user.AvatarUrl = !string.IsNullOrEmpty(avatarUrl) ? avatarUrl : $"local:{localFilePath}";
                        user.UpdatedAt = DateTime.UtcNow;
                        var success = await _dataService.UpdateUserAsync(user);
                        
                        if (success)
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ Аватар сохранен в профиль пользователя {userId}");
                            await Application.Current.MainPage.DisplayAlert("Успех", "Аватар изменен и сохранен", "OK");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сохранить аватар в профиль", "OK");
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Ошибка", "Пользователь не найден", "OK");
                    }
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

    private async Task LoadAvatarAsync()
    {
        try
        {
            if (!_authStateService.IsAuthenticated)
            {
                AvatarImage = "👤";
                return;
            }

            var userId = _authStateService.CurrentUserId;
            if (string.IsNullOrEmpty(userId))
            {
                AvatarImage = "👤";
                return;
            }

            var user = await _dataService.GetUserAsync(userId);
            
            if (user != null && !string.IsNullOrEmpty(user.AvatarUrl))
            {
                if (user.AvatarUrl.StartsWith("local:"))
                {
                    var localPath = user.AvatarUrl.Substring(6); // Убираем префикс "local:"
                    if (File.Exists(localPath))
                    {
                        AvatarPath = localPath;
                        AvatarImage = ImageSource.FromFile(localPath);
                        System.Diagnostics.Debug.WriteLine($"📸 Аватар загружен из локального хранилища: {localPath}");
                        return;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Локальный файл не найден: {localPath}");
                    }
                }
                else
                {
                    try
                    {
                        AvatarImage = ImageSource.FromUri(new Uri(user.AvatarUrl));
                        System.Diagnostics.Debug.WriteLine($"📸 Аватар загружен из URL: {user.AvatarUrl}");
                        
                        var localPath = Path.Combine(FileSystem.AppDataDirectory, "Avatars", $"avatar_{userId}.jpg");
                        if (File.Exists(localPath))
                        {
                            AvatarPath = localPath;
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Ошибка загрузки аватара из URL: {ex.Message}");
                        
                        var localPath = Path.Combine(FileSystem.AppDataDirectory, "Avatars", $"avatar_{userId}.jpg");
                        if (File.Exists(localPath))
                        {
                            AvatarPath = localPath;
                            AvatarImage = ImageSource.FromFile(localPath);
                            System.Diagnostics.Debug.WriteLine($"📸 Аватар загружен из локального хранилища (резервный): {localPath}");
                            return;
                        }
                    }
                }
            }
            
            var fallbackLocalPath = Path.Combine(FileSystem.AppDataDirectory, "Avatars", $"avatar_{userId}.jpg");
            if (File.Exists(fallbackLocalPath))
            {
                AvatarPath = fallbackLocalPath;
                AvatarImage = ImageSource.FromFile(fallbackLocalPath);
                System.Diagnostics.Debug.WriteLine($"📸 Аватар загружен из локального хранилища (прямая проверка): {fallbackLocalPath}");
                return;
            }
            
            AvatarImage = "👤";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки аватара: {ex.Message}");
            AvatarImage = "👤";
        }
    }
}
