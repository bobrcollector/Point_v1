public interface IAuthorizationService
{
    Task<bool> IsAdminAsync();
    Task<bool> IsModeratorAsync();
    Task<bool> CanModerateEventsAsync();
    Task<bool> CanManageUsersAsync();
    Task<UserRole> GetCurrentUserRoleAsync();
}