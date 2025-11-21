using Newtonsoft.Json;
using Point_v1.Models;

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
            var authResult = await _firebaseRest.CreateUserWithEmailAndPassword(email, password, displayName);
            if (authResult != null && !string.IsNullOrEmpty(authResult.IdToken))
            {
                _currentUserToken = authResult.IdToken;
                _currentUserId = authResult.LocalId;

           
                await CreateUserProfile(_currentUserId, email, displayName);

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

    private async Task CreateUserProfile(string userId, string email, string displayName)
    {
        try
        {
            var dataService = MauiProgram.CreateMauiApp().Services.GetService<IDataService>();
            if (dataService != null)
            {
                var user = new User
                {
                    Id = userId,
                    Email = email,
                    DisplayName = displayName,
                    City = "",
                    About = "",
                    AvatarUrl = "",
                    InterestIds = new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await dataService.UpdateUserAsync(user);
                System.Diagnostics.Debug.WriteLine($"✅ Профиль создан: {displayName}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Ошибка создания профиля: {ex.Message}");
        }
    }
    public async Task<bool> SignIn(string email, string password)
    {
        try
        {
            var authResult = await _firebaseRest.SignInWithEmailAndPassword(email, password);
            if (authResult != null && !string.IsNullOrEmpty(authResult.IdToken))
            {
                _currentUserToken = authResult.IdToken;
                _currentUserId = authResult.LocalId;
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

    public async Task<bool> DeleteAccountAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentUserToken))
            {
                System.Diagnostics.Debug.WriteLine("❌ Токен не найден - невозможно удалить аккаунт");
                return false;
            }

            System.Diagnostics.Debug.WriteLine("🗑️ Начало процесса удаления аккаунта");
            try
            {
                var dataService = MauiProgram.CreateMauiApp().Services.GetService<IDataService>();
                if (dataService != null && !string.IsNullOrEmpty(_currentUserId))
                {
                    System.Diagnostics.Debug.WriteLine($"📥 Удаляем пользовательские данные для {_currentUserId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Ошибка удаления данных пользователя: {ex.Message}");
            }
            var success = await _firebaseRest.DeleteAccountAsync(_currentUserToken);

            if (success)
            {
                System.Diagnostics.Debug.WriteLine("✅ Аккаунт успешно удален");
                _currentUserToken = null;
                _currentUserId = null;
                await ClearSession();
                AuthStateChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Не удалось удалить аккаунт");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка при удалении аккаунта: {ex.Message}");
            return false;
        }
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