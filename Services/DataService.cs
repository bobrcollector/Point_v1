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

        // ОБНОВЛЕННЫЕ тестовые события с полными данными
        _events = new List<Event>
    {
        new Event
        {
            Id = "1",
            Title = "Вечер настольных игр в коворкинге",
            Description = "Приглашаем всех любителей настольных игр! Будем играть в Мафию, Каркассон, Монополию и другие игры. Приносите свои любимые игры! Напитки и закуски предоставляются. Вход свободный.",
            CategoryId = "Настольные игры",
            Address = "Коворкинг 'Space', ул. Центральная, 15",
            EventDate = DateTime.Now.AddDays(1).AddHours(19),
            CreatorId = "user1",
            CreatorName = "Анна Иванова",
            MaxParticipants = 20,
            ParticipantIds = new List<string> { "user1", "user2", "user3" }
        },
        new Event
        {
            Id = "2",
            Title = "Аниме-марафон: Наруто",
            Description = "Смотрим вместе классические серии Наруто! Приносите попкорн, хорошее настроение и любимых персонажей. Будем смотреть самые культовые серии, обсуждать сюжет и просто хорошо проводить время.",
            CategoryId = "Аниме",
            Address = "Антикафе 'Geek Room', пр. Победы, 28",
            EventDate = DateTime.Now.AddDays(3).AddHours(17),
            CreatorId = "user2",
            CreatorName = "Дмитрий Петров",
            MaxParticipants = 15,
            ParticipantIds = new List<string> { "user2", "user4" }
        },
        new Event
        {
            Id = "3",
            Title = "Воркшоп по цифровому рисунку",
            Description = "Учимся основам digital art в Photoshop. Подходит для начинающих. Рассмотрим базовые инструменты, работу со слоями и создание простых иллюстраций. Приносите ноутбуки и графические планшеты.",
            CategoryId = "Искусство",
            Address = "Студия 'ArtSpace', ул. Творческая, 7",
            EventDate = DateTime.Now.AddDays(5).AddHours(15),
            CreatorId = "user3",
            CreatorName = "Мария Сидорова",
            MaxParticipants = 10,
            ParticipantIds = new List<string> { "user3" }
        }
    };

        System.Diagnostics.Debug.WriteLine($"✅ Инициализировано {_events.Count} тестовых событий");
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
        try
        {
            await Task.Delay(100);
            System.Diagnostics.Debug.WriteLine($"🔍 DataService.GetEventAsync вызван с eventId: {eventId}");

            var eventItem = _events.FirstOrDefault(e => e.Id == eventId);

            if (eventItem != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Событие найдено: {eventItem.Title}");
                System.Diagnostics.Debug.WriteLine($"📊 Участников: {eventItem.ParticipantIds?.Count ?? 0}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ Событие {eventId} НЕ найдено!");
            }

            return eventItem;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"💥 Ошибка в GetEventAsync: {ex.Message}");
            return null;
        }
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
        try
        {
            await Task.Delay(100);
            var eventItem = _events.FirstOrDefault(e => e.Id == eventId);

            if (eventItem != null && !eventItem.ParticipantIds.Contains(userId))
            {
                eventItem.ParticipantIds.Add(userId);
                System.Diagnostics.Debug.WriteLine($"✅ Пользователь {userId} присоединился к событию {eventId}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка присоединения: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> LeaveEventAsync(string eventId, string userId)
    {
        try
        {
            await Task.Delay(100);
            var eventItem = _events.FirstOrDefault(e => e.Id == eventId);

            if (eventItem != null)
            {
                eventItem.ParticipantIds.Remove(userId);
                System.Diagnostics.Debug.WriteLine($"✅ Пользователь {userId} вышел из события {eventId}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка выхода: {ex.Message}");
            return false;
        }
    }


}