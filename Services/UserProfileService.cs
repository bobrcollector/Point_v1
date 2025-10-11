using Point_v1.Models;

namespace Point_v1.Services;

public class UserProfileService
{
    private readonly IDataService _dataService;

    public UserProfileService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<bool> CreateUserProfile(string userId, string email, string displayName)
    {
        try
        {
            var user = new User
            {
                Id = userId,
                Email = email,
                DisplayName = displayName,
                City = "",
                About = "",
                InterestIds = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedEventsCount = 0,
                ParticipatedEventsCount = 0
            };

            var success = await _dataService.UpdateUserAsync(user);
            
            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Профиль создан: {displayName} ({userId})");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания профиля");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания профиля: {ex.Message}");
            return false;
        }
    }
}