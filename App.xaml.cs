using Point_v1.Services;

namespace Point_v1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        try
        {
            // Ждем инициализации MauiApp перед получением сервисов
            this.HandlerChanged += OnHandlerChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в конструкторе App: {ex.Message}");
        }
    }

    private void OnHandlerChanged(object sender, EventArgs e)
    {
        try
        {
            this.HandlerChanged -= OnHandlerChanged;

            var authorizationService = Handler.MauiContext.Services.GetService<IAuthorizationService>();
            var authStateService = Handler.MauiContext.Services.GetService<IAuthStateService>();

            MainPage = new AppShell(authorizationService, authStateService);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания AppShell: {ex.Message}");
            // Резервный вариант - создаем без зависимостей
            MainPage = new AppShell();
        }
    }
}