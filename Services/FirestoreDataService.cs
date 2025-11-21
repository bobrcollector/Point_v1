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

            var currentUser = await GetUserAsync(_authService.CurrentUserId);
            eventItem.CreatorName = currentUser?.DisplayName ?? "Организатор";
        }

        return await _firebaseRest.AddEventAsync(eventItem);
    }

    public async Task<List<Event>> GetUserEventsAsync(string userId)
    {
        try
        {
            var events = await GetEventsAsync();
            var userEvents = events.Where(e => e.CreatorId == userId && (e.IsActive || e.IsBlocked)).ToList();
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
                e.CreatorId != userId &&
                e.IsActive &&
                e.EventDate > DateTime.Now
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
            var archivedEvents = events.Where(e =>
                (e.EventDate < DateTime.Now || e.IsBlocked) &&
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

    public async Task<bool> JoinEventAsync(string eventId, string userId)
    {
        try
        {
            var events = await GetEventsAsync();
            var eventItem = events.FirstOrDefault(e => e.Id == eventId);

            if (eventItem != null && !eventItem.ParticipantIds.Contains(userId))
            {
                eventItem.ParticipantIds.Add(userId);
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

    public async Task<bool> UpdateEventAsync(Event eventItem)
    {
        try
        {
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
            return await _firebaseRest.DeleteEventAsync(eventId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка удаления события: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> BlockEventAsync(string eventId, string moderatorId, string reason)
    {
        try
        {
            var eventItem = await GetEventAsync(eventId);
            if (eventItem == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Событие с ID {eventId} не найдено для блокировки");
                return false;
            }

            eventItem.IsBlocked = true;
            eventItem.BlockedBy = moderatorId;
            eventItem.BlockedAt = DateTime.Now;
            eventItem.BlockReason = reason;
            eventItem.IsActive = false;

            System.Diagnostics.Debug.WriteLine($"🔒 Блокировка события {eventId} модератором {moderatorId}: {reason}");

            return await UpdateEventAsync(eventItem);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка блокировки события: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Interest>> GetInterestsAsync()
    {
        return await Task.FromResult(GetDefaultInterests());
    }

    public Task<bool> AddInterestAsync(Interest interest) => Task.FromResult(true);
    public async Task<List<Event>> GetEventsByInterestAsync(string interestId)
    {
        var events = await GetEventsAsync();
        return events.Where(e => e.CategoryId == interestId || 
                                 (e.CategoryIds != null && e.CategoryIds.Contains(interestId))).ToList();
    }

    public async Task<User> GetUserAsync(string userId)
    {
        try
        {
            var users = await GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == userId);
            
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Пользователь {userId} не найден в базе, создаем default");
                user = new User { Id = userId, DisplayName = "Пользователь", Role = UserRole.User };
            }
            
            if (userId == "test_moderator" || userId == "admin_test")
            {
                user.Role = UserRole.Admin;
                System.Diagnostics.Debug.WriteLine($"🧪 ТЕСТ: Пользователь {userId} установлен как Admin для тестирования");
            }
            
            System.Diagnostics.Debug.WriteLine($"✅ Пользователь: {user.DisplayName}, Role = {user.Role}");
            
            return user;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки пользователя: {ex.Message}");
            return new User { Id = userId, DisplayName = "Пользователь", Role = UserRole.User };
        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
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
            var users = await _firebaseRest.GetUsersAsync();
            foreach (var user in users)
            {
                System.Diagnostics.Debug.WriteLine($"👤 Пользователь из Firebase: {user.Id}, Role: {user.Role}");
            }
            
            return users;
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
            new Interest { Id = "15", Name = "🚗 Автомобили" },
            new Interest { Id = "16", Name = "🏥 Медицина" },
            new Interest { Id = "17", Name = "📌 Прочее" }
        };
    }
    public async Task<int> GetUserCreatedEventsCountAsync(string userId)
    {
        try
        {
            var events = await GetEventsAsync();
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
            return events.Count(e =>
                e.ParticipantIds != null &&
                e.ParticipantIds.Contains(userId) &&
                e.CreatorId != userId &&
                e.IsActive &&
                e.EventDate < DateTime.Now
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
            return events.Count(e =>
                e.IsActive &&
                e.EventDate > DateTime.Now &&
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