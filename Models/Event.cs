namespace Point_v1.Models;

public class Event
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string CategoryId { get; set; } // Оставлено для обратной совместимости
    public List<string> CategoryIds { get; set; } = new List<string>(); // Новое поле для нескольких категорий
    public string Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string CreatorId { get; set; }
    public string CreatorName { get; set; }
    public int MaxParticipants { get; set; } = 20;
    public List<string> ParticipantIds { get; set; } = new List<string>();
    public bool IsActive { get; set; } = true;

    // ДОБАВЬТЕ ЭТУ СТРОКУ ДЛЯ СИСТЕМЫ ЖАЛОБ
    public string ModerationNotes { get; set; }
    
    // Поля для блокировки события
    public bool IsBlocked { get; set; } = false;
    public string BlockedBy { get; set; } // ID модератора/администратора
    public DateTime? BlockedAt { get; set; }
    public string BlockReason { get; set; } // Причина блокировки

    // Вычисляемые свойства
    public int ParticipantsCount => ParticipantIds?.Count ?? 0;
    public bool HasFreeSpots => ParticipantsCount < MaxParticipants;
    public string DateDisplay
    {
        get
        {
            var culture = new System.Globalization.CultureInfo("ru-RU");
            return EventDate.ToString("dd MMMM HH:mm", culture);
        }
    }
    public string ShortDescription => string.IsNullOrEmpty(Description)
        ? "Описание отсутствует"
        : (Description.Length > 100 ? Description.Substring(0, 100) + "..." : Description);

    // Новые свойства для деталей с проверками
    public string DetailedDateDisplay
    {
        get
        {
            var culture = new System.Globalization.CultureInfo("ru-RU");
            return EventDate.ToString("dd MMMM yyyy 'в' HH:mm", culture);
        }
    }
    public string ParticipantsDisplay => $"{ParticipantsCount} из {MaxParticipants}";
    public double ParticipationProgress => MaxParticipants > 0 ? (double)ParticipantsCount / MaxParticipants : 0;
    public bool IsFull => ParticipantsCount >= MaxParticipants;
    public bool IsRelevant { get; set; }

    // Свойство для быстрой проверки - является ли событие созданным пользователем
    public bool IsCreatedByUser(string userId) => CreatorId == userId;
    public bool ShowMyEventBadge { get; set; }
    public string EventTypeText { get; set; } = string.Empty;
    
    // Свойство для проверки завершенности события (включая блокировку)
    public bool IsCompleted => EventDate < DateTime.Now || IsBlocked;
    
    // Свойство для отображения статуса блокировки
    public string BlockStatusText => IsBlocked ? "🚫 Событие заблокировано модератором" : string.Empty;
    
    // Свойство для проверки возможности редактирования (для использования в UI)
    public bool CanEdit { get; set; }
    
    // Свойство для отображения всех категорий
    public string CategoriesDisplay
    {
        get
        {
            if (CategoryIds != null && CategoryIds.Count > 0)
            {
                return string.Join(", ", CategoryIds);
            }
            else if (!string.IsNullOrEmpty(CategoryId))
            {
                return CategoryId;
            }
            return "Без категории";
        }
    }
    
    // Свойство для получения списка категорий для отображения
    public List<string> DisplayCategories
    {
        get
        {
            var result = new List<string>();
            
            if (CategoryIds != null && CategoryIds.Count > 0)
            {
                // Фильтруем пустые строки и возвращаем только валидные названия
                var validCategories = CategoryIds.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
                if (validCategories.Count > 0)
                {
                    result.AddRange(validCategories);
                    System.Diagnostics.Debug.WriteLine($"📋 DisplayCategories из CategoryIds: {string.Join(", ", result)}");
                    return result;
                }
            }
            
            // Если CategoryIds пустые или содержат только пустые строки, используем CategoryId
            if (!string.IsNullOrWhiteSpace(CategoryId))
            {
                result.Add(CategoryId);
                System.Diagnostics.Debug.WriteLine($"📋 DisplayCategories из CategoryId: {CategoryId}");
                return result;
            }
            
            System.Diagnostics.Debug.WriteLine($"📋 DisplayCategories пустой");
            return result;
        }
    }
    
    // Свойство для отображения первой категории с количеством остальных
    public string CategoryDisplay
    {
        get
        {
            var categories = DisplayCategories;
            if (categories == null || categories.Count == 0)
            {
                return "Без категории";
            }
            
            if (categories.Count == 1)
            {
                return categories[0];
            }
            
            // Показываем первую категорию + количество остальных
            var additionalCount = categories.Count - 1;
            return $"{categories[0]} +{additionalCount}";
        }
    }
}