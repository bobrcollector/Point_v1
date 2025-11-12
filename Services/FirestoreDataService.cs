using System.Text;
using Newtonsoft.Json;
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
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔄 Поиск события по ID: {eventId}");

            var events = await GetEventsAsync();
            var eventItem = events.FirstOrDefault(e => e.Id == eventId);

            if (eventItem == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Событие с ID {eventId} не найдено");
                System.Diagnostics.Debug.WriteLine($"📋 Доступные события: {events.Count} шт.");
                foreach (var ev in events.Take(5)) 
                {
                    System.Diagnostics.Debug.WriteLine($"   - {ev.Id}: {ev.Title}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"✅ Событие найдено: {eventItem.Title}");
            }

            return eventItem;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка поиска события: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> AddEventAsync(Event eventItem)
    {
        if (string.IsNullOrEmpty(eventItem.CreatorId) && _authService.IsAuthenticated)
        {
            eventItem.CreatorId = _authService.CurrentUserId;

            // ИСПРАВЛЕНИЕ: получаем пользователя по ID
            var currentUser = await GetUserAsync(_authService.CurrentUserId);
            eventItem.CreatorName = currentUser?.DisplayName ?? "Организатор";
        }

        return await _firebaseRest.AddEventAsync(eventItem);
    }

    // НОВЫЕ МЕТОДЫ ДЛЯ МОИХ СОБЫТИЙ
    public async Task<List<Event>> GetUserEventsAsync(string userId)
    {
        try
        {
            var events = await GetEventsAsync();
            // Все активные созданные события (и прошлые и будущие)
            var userEvents = events.Where(e => e.CreatorId == userId && e.IsActive).ToList();
            System.Diagnostics.Debug.WriteLine($"📥 Загружено созданных событий пользователя: {userEvents.Count}");
            return userEvents;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки созданных событий: {ex.Message}");
            return new List<Event>();
        }
    }

    public async Task<List<Event>> GetParticipatingEventsAsync(string userId)
    {
        try
        {
            var events = await GetEventsAsync();
            var participatingEvents = events.Where(e =>
                e.ParticipantIds.Contains(userId) &&
                e.CreatorId != userId && // исключаем события, где пользователь создатель
                e.IsActive &&
                e.EventDate > DateTime.Now // только БУДУЩИЕ события для участия
            ).ToList();

            System.Diagnostics.Debug.WriteLine($"📥 Загружено событий для участия: {participatingEvents.Count}");
            return participatingEvents;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки событий для участия: {ex.Message}");
            return new List<Event>();
        }
    }

    public async Task<List<Event>> GetArchivedEventsAsync(string userId)
    {
        try
        {
            var events = await GetEventsAsync();

            // ВКЛЮЧАЕМ В АРХИВ:
            // 1. Созданные пользователем И завершенные события
            // 2. События, в которых пользователь участвовал И которые завершены
            var archivedEvents = events.Where(e =>
                e.EventDate < DateTime.Now && // ТОЛЬКО ЗАВЕРШЕННЫЕ события
                (e.CreatorId == userId ||
                 (e.ParticipantIds != null && e.ParticipantIds.Contains(userId)))
            ).ToList();

            System.Diagnostics.Debug.WriteLine($"📥 Загружено архивных событий: {archivedEvents.Count}");
            return archivedEvents;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки архивных событий: {ex.Message}");
            return new List<Event>();
        }
    }

    // РЕАЛИЗАЦИЯ МЕТОДОВ УЧАСТИЯ В СОБЫТИЯХ
    public async Task<bool> JoinEventAsync(string eventId, string userId)
    {
        try
        {
            var events = await GetEventsAsync();
            var eventItem = events.FirstOrDefault(e => e.Id == eventId);

            if (eventItem != null && !eventItem.ParticipantIds.Contains(userId))
            {
                eventItem.ParticipantIds.Add(userId);
                // Обновляем событие в базе данных через FirebaseRestService
                return await _firebaseRest.UpdateEventAsync(eventItem);
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка присоединения к событию: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> LeaveEventAsync(string eventId, string userId)
    {
        try
        {
            var events = await GetEventsAsync();
            var eventItem = events.FirstOrDefault(e => e.Id == eventId);

            if (eventItem != null && eventItem.ParticipantIds.Contains(userId))
            {
                eventItem.ParticipantIds.Remove(userId);
                // Обновляем событие в базе данных через FirebaseRestService
                return await _firebaseRest.UpdateEventAsync(eventItem);
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка выхода из события: {ex.Message}");
            return false;
        }
    }

    // ИСПРАВЛЕННЫЕ МЕТОДЫ ДЛЯ РЕДАКТИРОВАНИЯ И УДАЛЕНИЯ
    public async Task<bool> UpdateEventAsync(Event eventItem)
    {
        try
        {
            // Используем FirebaseRestService для обновления
            return await _firebaseRest.UpdateEventAsync(eventItem);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления события: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteEventAsync(string eventId)
    {
        try
        {
            // Используем FirebaseRestService для удаления
            return await _firebaseRest.DeleteEventAsync(eventId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка удаления события: {ex.Message}");
            return false;
        }
    }

    // СУЩЕСТВУЮЩИЕ МЕТОДЫ
    public async Task<List<Interest>> GetInterestsAsync()
    {
        return await Task.FromResult(GetDefaultInterests());
    }

    public Task<bool> AddInterestAsync(Interest interest) => Task.FromResult(true);
    public Task<List<Event>> GetEventsByInterestAsync(string interestId) => Task.FromResult(new List<Event>());

    // РЕАЛЬНАЯ РЕАЛИЗАЦИЯ для работы с пользователями
    public async Task<User> GetUserAsync(string userId)
    {
        try
        {
            // Используем FirebaseRestService для получения пользователя
            var users = await GetAllUsersAsync();
            return users.FirstOrDefault(u => u.Id == userId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки пользователя: {ex.Message}");
            return new User { Id = userId, DisplayName = "Пользователь" };
        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            // Используем FirebaseRestService для сохранения
            return await _firebaseRest.AddOrUpdateUserAsync(user);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления пользователя: {ex.Message}");
            return false;
        }
    }

    private async Task<List<User>> GetAllUsersAsync()
    {
        try
        {
            return await _firebaseRest.GetUsersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки пользователей: {ex.Message}");
            return new List<User>();
        }
    }

    private List<Interest> GetDefaultInterests()
    {
        return new List<Interest>
        {
            new Interest { Id = "1", Name = "🎲 Настольные игры" },
            new Interest { Id = "2", Name = "🎭 Косплей" },
            new Interest { Id = "3", Name = "🎨 Искусство" },
            new Interest { Id = "4", Name = "💻 Программирование" },
            new Interest { Id = "5", Name = "📺 Аниме" },
            new Interest { Id = "6", Name = "📚 Комиксы" },
            new Interest { Id = "7", Name = "🎬 Кино" },
            new Interest { Id = "8", Name = "🎵 Музыка" },
            new Interest { Id = "9", Name = "⚽ Спорт" },
            new Interest { Id = "10", Name = "✈️ Путешествия" },
            new Interest { Id = "11", Name = "🍳 Кулинария" },
            new Interest { Id = "12", Name = "📸 Фотография" },
            new Interest { Id = "13", Name = "🎮 Видеоигры" },
            new Interest { Id = "14", Name = "📖 Книги" },
            new Interest { Id = "15", Name = "🚗 Автомобили" }
        };
    }
    public async Task<int> GetUserCreatedEventsCountAsync(string userId)
    {
        try
        {
            var events = await GetEventsAsync();
            // ПРОШЕДШИЕ созданные события
            return events.Count(e =>
                e.CreatorId == userId &&
                e.IsActive &&
                e.EventDate < DateTime.Now // ТОЛЬКО ПРОШЕДШИЕ
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения количества созданных событий: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> GetUserParticipatedEventsCountAsync(string userId)
    {
        try
        {
            var events = await GetEventsAsync();
            // ПРОШЕДШИЕ события участия
            return events.Count(e =>
                e.ParticipantIds != null &&
                e.ParticipantIds.Contains(userId) &&
                e.CreatorId != userId && // исключаем события, где пользователь создатель
                e.IsActive &&
                e.EventDate < DateTime.Now // ТОЛЬКО ПРОШЕДШИЕ
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения количества участий: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> GetUserUpcomingEventsCountAsync(string userId)
    {
        try
        {
            var events = await GetEventsAsync();
            // БУДУЩИЕ события (созданные + участия)
            return events.Count(e =>
                e.IsActive &&
                e.EventDate > DateTime.Now && // ТОЛЬКО БУДУЩИЕ
                (e.CreatorId == userId ||
                 (e.ParticipantIds != null && e.ParticipantIds.Contains(userId)))
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения количества предстоящих событий: {ex.Message}");
            return 0;
        }
    }
}