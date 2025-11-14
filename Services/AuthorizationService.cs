using Point_v1.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthStateService _authStateService;
    private readonly IDataService _dataService;

    public AuthorizationService(IAuthStateService authStateService, IDataService dataService)
    {
        _authStateService = authStateService;
        _dataService = dataService;
    }

    public async Task<bool> IsAdminAsync()
    {
        return await GetCurrentUserRoleAsync() == UserRole.Admin;
    }

    public async Task<bool> IsModeratorAsync()
    {
        try
        {
            var role = await GetCurrentUserRoleAsync();
            var isModerator = role == UserRole.Moderator || role == UserRole.Admin;
            System.Diagnostics.Debug.WriteLine($"?? AuthorizationService.IsModeratorAsync: Role = {role}, IsModerator = {isModerator}");
            return isModerator;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка проверки прав модератора: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CanModerateEventsAsync()
    {
        return await IsModeratorAsync();
    }

    public async Task<bool> CanManageUsersAsync()
    {
        return await IsAdminAsync();
    }

    public async Task<UserRole> GetCurrentUserRoleAsync()
    {
        if (!_authStateService.IsAuthenticated)
        {
            System.Diagnostics.Debug.WriteLine($"?? Пользователь не аутентифицирован");
            return UserRole.User;
        }

        var userId = _authStateService.CurrentUserId;
        System.Diagnostics.Debug.WriteLine($"?? Получение роли для пользователя: {userId}");

        try
        {
            var user = await _dataService.GetUserAsync(userId);
            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"? Роль пользователя {userId}: {user.Role}");
                return user.Role;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"?? Пользователь {userId} не найден, возвращаем User");
                return UserRole.User;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Ошибка получения роли: {ex.Message}");
            return UserRole.User;
        }
    }
}