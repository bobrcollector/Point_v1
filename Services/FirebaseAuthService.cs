namespace Point_v1.Services;

public class FirebaseAuthService : IAuthService
{
    private readonly FirebaseRestService _firebaseRest;
    private string _currentUserId;
    private string _currentUserToken;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_currentUserToken);
    public string CurrentUserId => _currentUserId;

    public event EventHandler AuthStateChanged;

    public FirebaseAuthService()
    {
        _firebaseRest = new FirebaseRestService();
        _ = RestoreSession();
    }

    public async Task<bool> CreateUser(string email, string password, string displayName)
    {
        try
        {
            var token = await _firebaseRest.CreateUserWithEmailAndPassword(email, password, displayName);
            if (!string.IsNullOrEmpty(token))
            {
                _currentUserToken = token;
                // Получаем userId из токена (в реальном приложении нужно парсить JWT)
                _currentUserId = Guid.NewGuid().ToString(); // Временное решение

                await SaveSession();
                AuthStateChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка регистрации: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SignIn(string email, string password)
    {
        try
        {
            var token = await _firebaseRest.SignInWithEmailAndPassword(email, password);
            if (!string.IsNullOrEmpty(token))
            {
                _currentUserToken = token;
                _currentUserId = Guid.NewGuid().ToString(); // Временное решение

                await SaveSession();
                AuthStateChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка входа: {ex.Message}");
            return false;
        }
    }

    public async Task SignOut()
    {
        _currentUserToken = null;
        _currentUserId = null;
        await ClearSession();
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task RestoreSession()
    {
        try
        {
            _currentUserToken = await SecureStorage.GetAsync("firebase_token");
            _currentUserId = await SecureStorage.GetAsync("user_id");

            if (!string.IsNullOrEmpty(_currentUserToken))
            {
                AuthStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка восстановления сессии: {ex.Message}");
        }
    }

    private async Task SaveSession()
    {
        if (!string.IsNullOrEmpty(_currentUserToken))
        {
            await SecureStorage.SetAsync("firebase_token", _currentUserToken);
            await SecureStorage.SetAsync("user_id", _currentUserId);
        }
    }

    private async Task ClearSession()
    {
        SecureStorage.Remove("firebase_token");
        SecureStorage.Remove("user_id");
    }
}