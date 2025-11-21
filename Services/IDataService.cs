using Point_v1.Models;

namespace Point_v1.Services;

public interface IDataService
{
    Task<List<Interest>> GetInterestsAsync();
    Task<bool> AddInterestAsync(Interest interest);
    Task<List<Event>> GetEventsAsync();
    Task<List<Event>> GetEventsByInterestAsync(string interestId);
    Task<Event> GetEventAsync(string eventId);
    Task<bool> AddEventAsync(Event eventItem);
    Task<bool> UpdateEventAsync(Event eventItem);
    Task<bool> DeleteEventAsync(string eventId);
    Task<bool> BlockEventAsync(string eventId, string moderatorId, string reason);
    Task<User> GetUserAsync(string userId);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> JoinEventAsync(string eventId, string userId);
    Task<bool> LeaveEventAsync(string eventId, string userId);
    Task<List<Event>> GetUserEventsAsync(string userId);
    Task<List<Event>> GetParticipatingEventsAsync(string userId);
    Task<List<Event>> GetArchivedEventsAsync(string userId);
    Task<int> GetUserCreatedEventsCountAsync(string userId);
    Task<int> GetUserParticipatedEventsCountAsync(string userId);
    Task<int> GetUserUpcomingEventsCountAsync(string userId);
}
