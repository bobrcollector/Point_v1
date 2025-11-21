namespace Point_v1.Services;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    string CurrentUserId { get; }
    Task<bool> CreateUser(string email, string password, string displayName);
    Task<bool> SignIn(string email, string password);
    Task SignOut();
    Task<bool> DeleteAccountAsync();
    event EventHandler AuthStateChanged;
}
