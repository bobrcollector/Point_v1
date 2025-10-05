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
                Title = "Играем в нарды. тест",
                Description = "Тут есть нарды, арбузы и чурчхела",
                CategoryId = "Настольные игры",
                Address = "армянская ул",
                EventDate = DateTime.Now.AddDays(1),
                CreatorId = "user1",
                ParticipantIds = new List<string> { "user1", "user2" }
            },
            new Event
            {
                Id = "2",
                Title = "Играем в камень ножницы бумага. тест",
                Description = "приходите играть в супер игру!!!",
                CategoryId = "Настольные игры",
                Address = "Метро Текстильщики",
                EventDate = DateTime.Now.AddDays(3),
                CreatorId = "user2",
                ParticipantIds = new List<string> { "user2" }
            }
        };
    }

    public async Task<List<Interest>> GetInterestsAsync()
    {
        await Task.Delay(100); 
        return _interests;
    }

    public async Task<bool> AddInterestAsync(Interest interest)
    {
        await Task.Delay(100);
        interest.Id = Guid.NewGuid().ToString();
        _interests.Add(interest);
        return true;
    }

    public async Task<Event> GetEventAsync(string eventId)
    {
        await Task.Delay(100);
        System.Diagnostics.Debug.WriteLine($"колво событий на данный момент: {_events.Count}");
        return _events.FirstOrDefault(e => e.Id == eventId);
    }

    public async Task<List<Event>> GetEventsByInterestAsync(string interestId)
    {
        await Task.Delay(100);
        return _events.Where(e => e.CategoryId == interestId).ToList();
    }

    public async Task<List<Event>> GetEventsAsync()
    {
        await Task.Delay(100);
        //System.Diagnostics.Debug.WriteLine($"📥 Загружено событий: {_events.Count}");
        return _events;
    }
    public async Task<bool> AddEventAsync(Event eventItem)
    {
        try
        {
            await Task.Delay(100);

            eventItem.Id = Guid.NewGuid().ToString();
            eventItem.CreatorId = _authStateService.CurrentUserId;
            eventItem.CreatorName = "тестовый организатор";
            eventItem.ParticipantIds = new List<string> { _authStateService.CurrentUserId };

            _events.Add(eventItem);

            //System.Diagnostics.Debug.WriteLine($"✅ Событие создано: {eventItem.Title}");
            //System.Diagnostics.Debug.WriteLine($"📊 Всего событий: {_events.Count}");

            System.Diagnostics.Debug.WriteLine($"событие сохранилось в бд!!!: {eventItem.Title}");
            System.Diagnostics.Debug.WriteLine($"айди: {eventItem.Id}");
            System.Diagnostics.Debug.WriteLine($"дата: {eventItem.EventDate}");

            return true;
        }
        catch (Exception ex)
        {
            //System.Diagnostics.Debug.WriteLine($"❌ Ошибка добавления события: {ex.Message}");
            return false;
        }
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
        return new User { Id = userId, DisplayName = "тестовый пользователь", Email = "test@mail.ru" };
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