using Point_v1.Services;

namespace Point_v1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        try
        {
            System.Diagnostics.Debug.WriteLine("🚀 App инициализируется");

            // Запрашиваем необходимые разрешения (не ждем результат)
            _ = RequestPermissionsAsync();

            // Ждем инициализации MauiApp перед получением сервисов
            this.HandlerChanged += OnHandlerChanged;
            
            System.Diagnostics.Debug.WriteLine("✅ App конструктор завершен успешно");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Критическая ошибка в конструкторе App: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        }
    }

    private async Task RequestPermissionsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("📍 Запрос разрешений на геолокацию");

            // Запрашиваем разрешения для доступа к геолокации
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            
            if (status != PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine($"📍 Текущий статус разрешения: {status}");
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                System.Diagnostics.Debug.WriteLine($"📍 Запрос разрешения на геолокацию: {status}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("📍 Разрешение на геолокацию уже выдано");
            }
        }
        catch (FeatureNotSupportedException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Разрешения не поддерживаются: {ex.Message}");
        }
        catch (PermissionException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка разрешения: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Неожиданная ошибка при запросе разрешений: {ex.Message}");
        }
    }

    private void OnHandlerChanged(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🎯 OnHandlerChanged вызван");
            this.HandlerChanged -= OnHandlerChanged;

            var authorizationService = Handler.MauiContext?.Services.GetService<IAuthorizationService>();
            var authStateService = Handler.MauiContext?.Services.GetService<IAuthStateService>();

            if (authorizationService != null && authStateService != null)
            {
                System.Diagnostics.Debug.WriteLine("✅ Сервисы получены, создаем AppShell");
                MainPage = new AppShell(authorizationService, authStateService);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Один из сервисов не найден, используем конструктор без параметров");
                MainPage = new AppShell();
            }

            System.Diagnostics.Debug.WriteLine("✅ MainPage установлена успешно");
            
            // Применяем сохраненную тему при запуске приложения
            ApplySavedTheme();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка при создании AppShell: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            
            try
            {
                // Резервный вариант - создаем без зависимостей
                MainPage = new AppShell();
                System.Diagnostics.Debug.WriteLine("✅ AppShell создана с резервным вариантом");
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ КРИТИЧЕСКАЯ ОШИБКА - даже резервный вариант не сработал: {fallbackEx.Message}");
            }
        }
    }

    private void ApplySavedTheme()
    {
        try
        {
            var savedTheme = Preferences.Get("AppTheme", "System");
            Application.Current.UserAppTheme = savedTheme switch
            {
                "Dark" => AppTheme.Dark,
                "Light" => AppTheme.Light,
                _ => AppTheme.Unspecified // System default
            };
            System.Diagnostics.Debug.WriteLine($"🌙 Тема применена при запуске: {savedTheme}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка применения темы при запуске: {ex.Message}");
        }
    }
}