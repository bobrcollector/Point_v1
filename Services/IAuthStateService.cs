namespace Point_v1.Services;

public interface IAuthStateService
{
    bool IsAuthenticated { get; }
    string CurrentUserId { get; }
    event EventHandler AuthenticationStateChanged;
}

public class AuthStateService : IAuthStateService
{
    private readonly IAuthService _authService;

    public AuthStateService(IAuthService authService)
    {
        _authService = authService;
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    public bool IsAuthenticated => _authService.IsAuthenticated;
    public string CurrentUserId => _authService.CurrentUserId;

    public event EventHandler AuthenticationStateChanged;

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        AuthenticationStateChanged?.Invoke(this, EventArgs.Empty);
    }
}