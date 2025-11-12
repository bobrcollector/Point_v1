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
        var role = await GetCurrentUserRoleAsync();
        return role == UserRole.Moderator || role == UserRole.Admin;
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
            return UserRole.User;

        var user = await _dataService.GetUserAsync(_authStateService.CurrentUserId);
        return user?.Role ?? UserRole.User;
    }
}