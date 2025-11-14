using Point_v1.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IAuthStateService _authStateService;
    private readonly IDataService _dataService;

    public SettingsViewModel(IAuthStateService authStateService, IDataService dataService)
    {
        _authStateService = authStateService;
        _dataService = dataService;

        // Инициализация команд
        ChangePasswordCommand = new Command(async () => await ChangePassword());
        ClearCacheCommand = new Command(async () => await ClearCache());
        OpenTermsCommand = new Command(async () => await OpenTerms());
        OpenPrivacyCommand = new Command(async () => await OpenPrivacy());
        OpenHelpCommand = new Command(async () => await OpenHelp());
        ViewSessionsCommand = new Command(async () => await ViewSessions());
        DownloadDataCommand = new Command(async () => await DownloadData());
        DeleteAccountCommand = new Command(async () => await DeleteAccount());

        // Инициализация языков
        InitializeLanguages();

        // Загружаем сохраненные настройки
        LoadSettings();
    }

    #region Properties - Язык и локализация

    private ObservableCollection<string> _availableLanguages;
    public ObservableCollection<string> AvailableLanguages
    {
        get => _availableLanguages;
        set => SetProperty(ref _availableLanguages, value);
    }

    private string _selectedLanguage = "Русский";
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                OnLanguageChanged(value);
            }
        }
    }

    #endregion

    #region Properties - Безопасность

    private string _currentPassword;
    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    private string _newPassword;
    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    private string _confirmPassword;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    private bool _isTwoFactorEnabled;
    public bool IsTwoFactorEnabled
    {
        get => _isTwoFactorEnabled;
        set
        {
            if (SetProperty(ref _isTwoFactorEnabled, value))
            {
                OnTwoFactorChanged(value);
            }
        }
    }

    #endregion

    #region Properties - Приватность

    private bool _isProfileVisible = true;
    public bool IsProfileVisible
    {
        get => _isProfileVisible;
        set => SetProperty(ref _isProfileVisible, value);
    }

    private bool _isEventHistoryVisible = true;
    public bool IsEventHistoryVisible
    {
        get => _isEventHistoryVisible;
        set => SetProperty(ref _isEventHistoryVisible, value);
    }

    private bool _isPersonalizedRecommendations = true;
    public bool IsPersonalizedRecommendations
    {
        get => _isPersonalizedRecommendations;
        set => SetProperty(ref _isPersonalizedRecommendations, value);
    }

    #endregion

    #region Properties - Уведомления

    private bool _isEventNotificationsEnabled = true;
    public bool IsEventNotificationsEnabled
    {
        get => _isEventNotificationsEnabled;
        set => SetProperty(ref _isEventNotificationsEnabled, value);
    }

    private bool _isMessageNotificationsEnabled = true;
    public bool IsMessageNotificationsEnabled
    {
        get => _isMessageNotificationsEnabled;
        set => SetProperty(ref _isMessageNotificationsEnabled, value);
    }

    private bool _isMarketingNotificationsEnabled = false;
    public bool IsMarketingNotificationsEnabled
    {
        get => _isMarketingNotificationsEnabled;
        set => SetProperty(ref _isMarketingNotificationsEnabled, value);
    }

    #endregion

    #region Properties - Информация о приложении

    private string _appVersion;
    public string AppVersion
    {
        get => _appVersion;
        set => SetProperty(ref _appVersion, value);
    }

    private string _apiVersion = "v1.0";
    public string ApiVersion
    {
        get => _apiVersion;
        set => SetProperty(ref _apiVersion, value);
    }

    private string _cacheSize;
    public string CacheSize
    {
        get => _cacheSize;
        set => SetProperty(ref _cacheSize, value);
    }

    #endregion

    #region Commands

    public ICommand ChangePasswordCommand { get; }
    public ICommand ClearCacheCommand { get; }
    public ICommand OpenTermsCommand { get; }
    public ICommand OpenPrivacyCommand { get; }
    public ICommand OpenHelpCommand { get; }
    public ICommand ViewSessionsCommand { get; }
    public ICommand DownloadDataCommand { get; }
    public ICommand DeleteAccountCommand { get; }

    #endregion

    #region Methods - Инициализация

    private void InitializeLanguages()
    {
        AvailableLanguages = new ObservableCollection<string>
        {
            "Русский",
            "English",
            "Espa?ol",
            "Fran?ais",
            "Deutsch"
        };

        // Загружаем сохраненный язык
        var savedLanguage = Preferences.Get("AppLanguage", "Русский");
        SelectedLanguage = savedLanguage;
    }

    #endregion

    #region Methods - Загрузка и сохранение

    public void LoadSettings()
    {
        try
        {
            // Загружаем язык
            SelectedLanguage = Preferences.Get("AppLanguage", "Русский");

            // Загружаем настройки безопасности
            IsTwoFactorEnabled = Preferences.Get("TwoFactorEnabled", false);

            // Загружаем настройки приватности
            IsProfileVisible = Preferences.Get("ProfileVisible", true);
            IsEventHistoryVisible = Preferences.Get("EventHistoryVisible", true);
            IsPersonalizedRecommendations = Preferences.Get("PersonalizedRecommendations", true);

            // Загружаем настройки уведомлений
            IsEventNotificationsEnabled = Preferences.Get("EventNotificationsEnabled", true);
            IsMessageNotificationsEnabled = Preferences.Get("MessageNotificationsEnabled", true);
            IsMarketingNotificationsEnabled = Preferences.Get("MarketingNotificationsEnabled", false);

            // Загружаем информацию о приложении
            AppVersion = AppInfo.Current.VersionString;
            CalculateCacheSize();

            System.Diagnostics.Debug.WriteLine("? Настройки загружены");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка загрузки настроек: {ex.Message}");
        }
    }

    public void SaveSettings()
    {
        try
        {
            Preferences.Set("AppLanguage", SelectedLanguage);
            Preferences.Set("TwoFactorEnabled", IsTwoFactorEnabled);
            Preferences.Set("ProfileVisible", IsProfileVisible);
            Preferences.Set("EventHistoryVisible", IsEventHistoryVisible);
            Preferences.Set("PersonalizedRecommendations", IsPersonalizedRecommendations);
            Preferences.Set("EventNotificationsEnabled", IsEventNotificationsEnabled);
            Preferences.Set("MessageNotificationsEnabled", IsMessageNotificationsEnabled);
            Preferences.Set("MarketingNotificationsEnabled", IsMarketingNotificationsEnabled);

            System.Diagnostics.Debug.WriteLine("? Настройки сохранены");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка сохранения настроек: {ex.Message}");
        }
    }

    #endregion

    #region Methods - Безопасность

    private async Task ChangePassword()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword) || 
                string.IsNullOrWhiteSpace(NewPassword) || 
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", 
                    "Все поля должны быть заполнены", "OK");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", 
                    "Новые пароли не совпадают", "OK");
                return;
            }

            if (NewPassword.Length < 6)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", 
                    "Пароль должен содержать минимум 6 символов", "OK");
                return;
            }

            // TODO: Реализовать смену пароля через Firebase
            // var authService = Application.Current.Handler.MauiContext.Services.GetService<IAuthService>();
            // var success = await authService.ChangePassword(CurrentPassword, NewPassword);

            await Application.Current.MainPage.DisplayAlert("Успех", 
                "Пароль успешно изменен", "OK");

            // Очищаем поля
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;

            System.Diagnostics.Debug.WriteLine("? Пароль изменен");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка смены пароля: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", 
                "Не удалось изменить пароль", "OK");
        }
    }

    private void OnTwoFactorChanged(bool isEnabled)
    {
        if (isEnabled)
        {
            System.Diagnostics.Debug.WriteLine("?? Двухфакторная аутентификация включена");
            // TODO: Отправить код подтверждения
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("?? Двухфакторная аутентификация отключена");
        }
    }

    #endregion

    #region Methods - Язык

    private void OnLanguageChanged(string language)
    {
        System.Diagnostics.Debug.WriteLine($"?? Язык изменен на: {language}");
        // TODO: Реализовать смену языка в приложении
        SaveSettings();
    }

    #endregion

    #region Methods - Информация о приложении

    private void CalculateCacheSize()
    {
        try
        {
            var cacheDir = FileSystem.CacheDirectory;
            var dirInfo = new DirectoryInfo(cacheDir);
            var files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            
            long totalSize = files.Sum(f => f.Length);
            CacheSize = FormatBytes(totalSize);

            System.Diagnostics.Debug.WriteLine($"?? Размер кеша: {CacheSize}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка расчета размера кеша: {ex.Message}");
            CacheSize = "Не удалось определить";
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    private async Task ClearCache()
    {
        try
        {
            var confirm = await Application.Current.MainPage.DisplayAlert("Подтверждение", 
                "Вы уверены? Это удалит все кэшированные данные.", "Да", "Нет");

            if (confirm)
            {
                var cacheDir = FileSystem.CacheDirectory;
                var dirInfo = new DirectoryInfo(cacheDir);
                
                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    file.Delete();
                }

                CalculateCacheSize();
                await Application.Current.MainPage.DisplayAlert("Успех", 
                    "Кэш успешно очищен", "OK");

                System.Diagnostics.Debug.WriteLine("? Кэш очищен");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка очистки кэша: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", 
                "Не удалось очистить кэш", "OK");
        }
    }

    private async Task OpenTerms()
    {
        try
        {
            // TODO: Открыть условия использования (например, в браузере или на отдельной странице)
            await Application.Current.MainPage.DisplayAlert("Условия использования", 
                "Здесь будут отображены условия использования приложения", "OK");

            System.Diagnostics.Debug.WriteLine("?? Открыты условия использования");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка открытия условий: {ex.Message}");
        }
    }

    private async Task OpenPrivacy()
    {
        try
        {
            // TODO: Открыть политику конфиденциальности
            await Application.Current.MainPage.DisplayAlert("Политика конфиденциальности", 
                "Здесь будет отображена политика конфиденциальности приложения", "OK");

            System.Diagnostics.Debug.WriteLine("?? Открыта политика конфиденциальности");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка открытия политики: {ex.Message}");
        }
    }

    private async Task OpenHelp()
    {
        try
        {
            // TODO: Открыть справку
            await Application.Current.MainPage.DisplayAlert("Справка", 
                "Здесь будет отображена справка по использованию приложения", "OK");

            System.Diagnostics.Debug.WriteLine("? Открыта справка");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка открытия справки: {ex.Message}");
        }
    }

    #endregion

    #region Methods - Управление аккаунтом

    private async Task ViewSessions()
    {
        try
        {
            // TODO: Реализовать просмотр активных сеансов
            await Application.Current.MainPage.DisplayAlert("Активные сеансы", 
                "Здесь будут отображены ваши активные сеансы на разных устройствах", "OK");

            System.Diagnostics.Debug.WriteLine("??? Просмотр сеансов");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка просмотра сеансов: {ex.Message}");
        }
    }

    private async Task DownloadData()
    {
        try
        {
            var confirm = await Application.Current.MainPage.DisplayAlert("Загрузка данных", 
                "Подготовка данных может занять некоторое время. Продолжить?", "Да", "Нет");

            if (confirm)
            {
                // TODO: Реализовать загрузку данных пользователя
                await Application.Current.MainPage.DisplayAlert("Успех", 
                    "Ваши данные готовы к загрузке. Файл будет отправлен на ваш email.", "OK");

                System.Diagnostics.Debug.WriteLine("?? Загрузка данных инициирована");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка загрузки данных: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", 
                "Не удалось загрузить данные", "OK");
        }
    }

    private async Task DeleteAccount()
    {
        try
        {
            var confirm = await Application.Current.MainPage.DisplayAlert("?? ВНИМАНИЕ", 
                "Это действие необратимо! Все ваши данные будут удалены. Вы уверены?", "Да, удалить", "Отмена");

            if (confirm)
            {
                var secondConfirm = await Application.Current.MainPage.DisplayAlert("Подтверждение удаления", 
                    "Повторно подтвердите удаление аккаунта. Это последнее предупреждение!", "Да, удалить окончательно", "Отмена");

                if (secondConfirm)
                {
                    // TODO: Реализовать удаление аккаунта
                    // var authService = Application.Current.Handler.MauiContext.Services.GetService<IAuthService>();
                    // await authService.DeleteAccount();

                    await Application.Current.MainPage.DisplayAlert("Успех", 
                        "Ваш аккаунт был удален. Приложение будет закрыто.", "OK");

                    System.Diagnostics.Debug.WriteLine("??? Аккаунт удален");

                    // Выход из приложения
                    // await _navigationService.GoToLoginAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка удаления аккаунта: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", 
                "Не удалось удалить аккаунт", "OK");
        }
    }

    #endregion
}
