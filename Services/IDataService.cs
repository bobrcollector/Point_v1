using Point_v1.Models;

namespace Point_v1.Services;

public interface IDataService
{
    // Interests
    Task<List<Interest>> GetInterestsAsync();
    Task<bool> AddInterestAsync(Interest interest);

    // Events
    Task<List<Event>> GetEventsAsync();
    Task<List<Event>> GetEventsByInterestAsync(string interestId);
    Task<Event> GetEventAsync(string eventId);
    Task<bool> AddEventAsync(Event eventItem);
    Task<bool> UpdateEventAsync(Event eventItem);
    Task<bool> DeleteEventAsync(string eventId);

    // Users
    Task<User> GetUserAsync(string userId);
    Task<bool> UpdateUserAsync(User user);

    // Event participation
    Task<bool> JoinEventAsync(string eventId, string userId);
    Task<bool> LeaveEventAsync(string eventId, string userId);
}