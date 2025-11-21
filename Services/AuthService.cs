using System.Text.Json;

namespace Point_v1.Services;

public class AuthService : IAuthService
{
    private bool _isAuthenticated = false;
    private string _currentUserId = string.Empty;
    private const string AuthKey = "user_auth";

    public bool IsAuthenticated => _isAuthenticated;
    public string CurrentUserId => _currentUserId;

    public event EventHandler AuthStateChanged;

    public AuthService()
    {
        LoadAuthState();
    }

    public async Task<bool> CreateUser(string email, string password, string displayName)
    {
        try
        {
            await Task.Delay(1500);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            _isAuthenticated = true;
            _currentUserId = Guid.NewGuid().ToString();
            SaveAuthState();
            AuthStateChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> SignIn(string email, string password)
    {
        try
        {
            await Task.Delay(1000);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            if (password != "123456")
                return false;

            _isAuthenticated = true;
            _currentUserId = "user_" + email.GetHashCode();
            SaveAuthState();
            AuthStateChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Task SignOut()
    {
        _isAuthenticated = false;
        _currentUserId = string.Empty;
        ClearAuthState();
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
    private void SaveAuthState()
    {
        try
        {
            var authData = new { IsAuthenticated = _isAuthenticated, UserId = _currentUserId };
            var json = JsonSerializer.Serialize(authData);
            Preferences.Set(AuthKey, json);
        }
        catch { }
    }

    private void LoadAuthState()
    {
        try
        {
            var json = Preferences.Get(AuthKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                var authData = JsonSerializer.Deserialize<AuthData>(json);
                if (authData != null)
                {
                    _isAuthenticated = authData.IsAuthenticated;
                    _currentUserId = authData.UserId;
                }
            }
        }
        catch { }
    }
    public async Task<bool> DeleteAccountAsync()
    {
        try
        {
            await Task.Delay(1000);
            _isAuthenticated = false;
            _currentUserId = string.Empty;
            ClearAuthState();
            AuthStateChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    private void ClearAuthState()
    {
        Preferences.Remove(AuthKey);
    }

    private class AuthData
    {
        public bool IsAuthenticated { get; set; }
        public string UserId { get; set; }
    }
}