using Point_v1.Models;

namespace Point_v1.Services;

public class FirestoreDataService : IDataService
{
    private readonly FirebaseRestService _firebaseRest;
    private readonly IAuthService _authService;

    public FirestoreDataService(IAuthService authService)
    {
        _authService = authService;
        _firebaseRest = new FirebaseRestService();
    }

    public async Task<List<Event>> GetEventsAsync()
    {
        return await _firebaseRest.GetEventsAsync();
    }

    public async Task<Event> GetEventAsync(string eventId)
    {
        var events = await GetEventsAsync();
        return events.FirstOrDefault(e => e.Id == eventId);
    }

    public async Task<bool> AddEventAsync(Event eventItem)
    {
        if (string.IsNullOrEmpty(eventItem.CreatorId) && _authService.IsAuthenticated)
        {
            eventItem.CreatorId = _authService.CurrentUserId;
        }

        return await _firebaseRest.AddEventAsync(eventItem);
    }

    // Временные заглушки для остальных методов
    public Task<List<Interest>> GetInterestsAsync() => Task.FromResult(GetDefaultInterests());
    public Task<bool> AddInterestAsync(Interest interest) => Task.FromResult(true);
    public Task<List<Event>> GetEventsByInterestAsync(string interestId) => Task.FromResult(new List<Event>());
    public Task<bool> UpdateEventAsync(Event eventItem) => Task.FromResult(true);
    public Task<bool> DeleteEventAsync(string eventId) => Task.FromResult(true);
    public Task<User> GetUserAsync(string userId) => Task.FromResult(new User { Id = userId, DisplayName = "Пользователь" });
    public Task<bool> UpdateUserAsync(User user) => Task.FromResult(true);
    public Task<bool> JoinEventAsync(string eventId, string userId) => Task.FromResult(true);
    public Task<bool> LeaveEventAsync(string eventId, string userId) => Task.FromResult(true);

    private List<Interest> GetDefaultInterests()
    {
        return new List<Interest>
        {
            new Interest { Id = "1", Name = "Настольные игры" },
            new Interest { Id = "2", Name = "Косплей" },
            new Interest { Id = "3", Name = "Искусство" },
            new Interest { Id = "4", Name = "Программирование" },
            new Interest { Id = "5", Name = "Аниме" }
        };
    }
}