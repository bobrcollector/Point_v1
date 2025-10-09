namespace Point_v1.Services;

public class AuthStateService : IAuthStateService
{
    private readonly IAuthService _authService;

    public AuthStateService(IAuthService authService)
    {
        _authService = authService;

        _authService.AuthStateChanged += (s, e) =>
        {
            AuthenticationStateChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    public bool IsAuthenticated => _authService.IsAuthenticated;
    public string CurrentUserId => _authService.CurrentUserId;

    public event EventHandler AuthenticationStateChanged;
}