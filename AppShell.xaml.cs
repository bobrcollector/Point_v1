using Point_v1.Services;
using Point_v1.ViewModels;

namespace Point_v1;

public partial class AppShell : Shell
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthStateService _authStateService;

    // Конструктор по умолчанию для дизайнера
    public AppShell() : this(null, null)
    {
    }

    public AppShell(IAuthorizationService authorizationService, IAuthStateService authStateService)
    {
        try
        {
            InitializeComponent();

            _authorizationService = authorizationService;
            _authStateService = authStateService;

            System.Diagnostics.Debug.WriteLine($"========== 🔧 AppShell инициализируется ==========");
            System.Diagnostics.Debug.WriteLine($"  - IAuthorizationService: {(_authorizationService != null ? "✅" : "❌")}");
            System.Diagnostics.Debug.WriteLine($"  - IAuthStateService: {(_authStateService != null ? "✅" : "❌")}");

            // Проверяем, что сервисы не null
            if (_authStateService != null)
            {
                _authStateService.AuthenticationStateChanged += OnAuthenticationStateChanged;
                System.Diagnostics.Debug.WriteLine("✅ Обработчик AuthenticationStateChanged подписан");
                _ = CheckAdminPermissions();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ AuthStateService is null - админ-панель отключена");
                AdminTab.IsVisible = false;
            }
            System.Diagnostics.Debug.WriteLine($"========== ✅ AppShell инициализация завершена ==========");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в конструкторе AppShell: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        }
    }

    private async void OnAuthenticationStateChanged(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("========== 🔄 AuthenticationStateChanged событие вызвано ==========");
        await CheckAdminPermissions();
    }

    private async Task CheckAdminPermissions()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"========== 🔍 Проверка прав модератора ==========");
            System.Diagnostics.Debug.WriteLine($"  - IsAuthenticated: {_authStateService?.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"  - CurrentUserId: {_authStateService?.CurrentUserId}");
            System.Diagnostics.Debug.WriteLine($"  - AuthorizationService != null: {_authorizationService != null}");

            if (_authStateService?.IsAuthenticated == true && _authorizationService != null)
            {
                var isModerator = await _authorizationService.IsModeratorAsync();
                System.Diagnostics.Debug.WriteLine($"✅ Проверка прав завершена: модератор = {isModerator}");
                AdminTab.IsVisible = isModerator;
                System.Diagnostics.Debug.WriteLine($"🛡️ AdminTab.IsVisible = {AdminTab.IsVisible}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Пользователь не аутентифицирован или сервис отсутствует");
                AdminTab.IsVisible = false;
            }
            System.Diagnostics.Debug.WriteLine($"========== ✅ Проверка прав завершена ==========");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка проверки прав: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            AdminTab.IsVisible = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_authStateService != null)
        {
            _authStateService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}