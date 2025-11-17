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
        // Пробуем восстановить состояние при запуске
        LoadAuthState();
    }

    public async Task<bool> CreateUser(string email, string password, string displayName)
    {
        try
        {
            await Task.Delay(1500); // Имитация регистрации

            // Валидация
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            // Создаем "нового пользователя"
            _isAuthenticated = true;
            _currentUserId = Guid.NewGuid().ToString();

            // Сохраняем состояние
            SaveAuthState();

            // Уведомляем об изменении
            AuthStateChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка регистрации: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SignIn(string email, string password)
    {
        try
        {
            await Task.Delay(1000); // Имитация входа

            // Валидация
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;

            // Простая проверка - любой email и пароль "123456"
            if (password != "123456")
                return false;

            _isAuthenticated = true;
            _currentUserId = "user_" + email.GetHashCode();

            // Сохраняем состояние
            SaveAuthState();

            AuthStateChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка входа: {ex.Message}");
            return false;
        }
    }

    public Task SignOut()
    {
        _isAuthenticated = false;
        _currentUserId = string.Empty;

        // Очищаем сохраненное состояние
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения состояния: {ex.Message}");
        }
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки состояния: {ex.Message}");
        }
    }
    public async Task<bool> DeleteAccountAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🗑️ Начало удаления аккаунта (тестовый режим)");

            // Имитация удаления аккаунта
            await Task.Delay(1000);

            // Очищаем состояние аутентификации
            _isAuthenticated = false;
            _currentUserId = string.Empty;

            // Очищаем сохраненное состояние
            ClearAuthState();

            // Уведомляем об изменении состояния
            AuthStateChanged?.Invoke(this, EventArgs.Empty);

            System.Diagnostics.Debug.WriteLine("✅ Аккаунт удален (тестовый режим)");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка удаления аккаунта: {ex.Message}");
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