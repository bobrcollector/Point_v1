using Point_v1.Models;

namespace Point_v1.Services;

public class DataService : IDataService
{
    private readonly IAuthStateService _authStateService;
    private List<Event> _events = new List<Event>();
    private List<Interest> _interests = new List<Interest>();

    public DataService(IAuthStateService authStateService)
    {
        _authStateService = authStateService;
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        // Тестовые интересы
        _interests = new List<Interest>
        {
            new Interest { Id = "1", Name = "Настольные игры" },
            new Interest { Id = "2", Name = "Косплей" },
            new Interest { Id = "3", Name = "Искусство" },
            new Interest { Id = "4", Name = "Программирование" },
            new Interest { Id = "5", Name = "Аниме" }
        };

        // Тестовые события
        _events = new List<Event>
        {
            new Event
            {
                Id = "1",
                Title = "Вечер настольных игр",
                Description = "Играем в Мафию, Каркассон и другие игры",
                CategoryId = "Настольные игры",
                Address = "Кафе 'Игровая зона'",
                EventDate = DateTime.Now.AddDays(1),
                CreatorId = "user1",
                ParticipantIds = new List<string> { "user1", "user2" }
            },
            new Event
            {
                Id = "2",
                Title = "Аниме-марафон",
                Description = "Смотрим классические аниме",
                CategoryId = "Аниме",
                Address = "Антикафе 'Geek Room'",
                EventDate = DateTime.Now.AddDays(3),
                CreatorId = "user2",
                ParticipantIds = new List<string> { "user2" }
            }
        };
    }

    public async Task<List<Interest>> GetInterestsAsync()
    {
        await Task.Delay(100); // Имитация загрузки
        return _interests;
    }

    public async Task<bool> AddInterestAsync(Interest interest)
    {
        await Task.Delay(100);
        interest.Id = Guid.NewGuid().ToString();
        _interests.Add(interest);
        return true;
    }

    public async Task<List<Event>> GetEventsAsync()
    {
        await Task.Delay(100);
        return _events;
    }

    public async Task<List<Event>> GetEventsByInterestAsync(string interestId)
    {
        await Task.Delay(100);
        return _events.Where(e => e.CategoryId == interestId).ToList();
    }

    public async Task<Event> GetEventAsync(string eventId)
    {
        await Task.Delay(100);
        return _events.FirstOrDefault(e => e.Id == eventId);
    }

    public async Task<bool> AddEventAsync(Event eventItem)
    {
        await Task.Delay(100);
        eventItem.Id = Guid.NewGuid().ToString();
        eventItem.CreatorId = _authStateService.CurrentUserId;
        eventItem.ParticipantIds = new List<string> { _authStateService.CurrentUserId };
        _events.Add(eventItem);
        return true;
    }

    public async Task<bool> UpdateEventAsync(Event eventItem)
    {
        await Task.Delay(100);
        var existingEvent = _events.FirstOrDefault(e => e.Id == eventItem.Id);
        if (existingEvent != null)
        {
            _events.Remove(existingEvent);
            _events.Add(eventItem);
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteEventAsync(string eventId)
    {
        await Task.Delay(100);
        var eventToRemove = _events.FirstOrDefault(e => e.Id == eventId);
        if (eventToRemove != null)
        {
            _events.Remove(eventToRemove);
            return true;
        }
        return false;
    }

    public async Task<User> GetUserAsync(string userId)
    {
        await Task.Delay(100);
        return new User { Id = userId, DisplayName = "Тестовый пользователь", Email = "test@example.com" };
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        await Task.Delay(100);
        return true;
    }

    public async Task<bool> JoinEventAsync(string eventId, string userId)
    {
        await Task.Delay(100);
        var eventItem = _events.FirstOrDefault(e => e.Id == eventId);
        if (eventItem != null && !eventItem.ParticipantIds.Contains(userId))
        {
            eventItem.ParticipantIds.Add(userId);
            return true;
        }
        return false;
    }

    public async Task<bool> LeaveEventAsync(string eventId, string userId)
    {
        await Task.Delay(100);
        var eventItem = _events.FirstOrDefault(e => e.Id == eventId);
        if (eventItem != null)
        {
            eventItem.ParticipantIds.Remove(userId);
            return true;
        }
        return false;
    }
}