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

            // Проверяем, что сервисы не null
            if (_authStateService != null)
            {
                _authStateService.AuthenticationStateChanged += OnAuthenticationStateChanged;
                _ = CheckAdminPermissions();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ AuthStateService is null - админ-панель отключена");
                AdminTab.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в конструкторе AppShell: {ex.Message}");
        }
    }

    private async void OnAuthenticationStateChanged(object sender, EventArgs e)
    {
        await CheckAdminPermissions();
    }

    private async Task CheckAdminPermissions()
    {
        try
        {
            if (_authStateService?.IsAuthenticated == true && _authorizationService != null)
            {
                var isModerator = await _authorizationService.IsModeratorAsync();
                AdminTab.IsVisible = isModerator;
                System.Diagnostics.Debug.WriteLine($"🛡️ Проверка прав: модератор = {isModerator}");
            }
            else
            {
                AdminTab.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка проверки прав: {ex.Message}");
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