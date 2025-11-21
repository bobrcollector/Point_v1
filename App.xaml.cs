using Point_v1.Services;

namespace Point_v1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        try
        {
            _ = RequestPermissionsAsync();
            this.HandlerChanged += OnHandlerChanged;
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
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
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
            this.HandlerChanged -= OnHandlerChanged;
            var authorizationService = Handler.MauiContext?.Services.GetService<IAuthorizationService>();
            var authStateService = Handler.MauiContext?.Services.GetService<IAuthStateService>();

            if (authorizationService != null && authStateService != null)
            {
                MainPage = new AppShell(authorizationService, authStateService);
            }
            else
            {
                MainPage = new AppShell();
            }

            ApplySavedTheme();
        }
        catch (Exception)
        {
            try
            {
                MainPage = new AppShell();
            }
            catch { }
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
                _ => AppTheme.Unspecified
            };
        }
        catch { }
    }
}