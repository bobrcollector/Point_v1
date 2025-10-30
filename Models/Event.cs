namespace Point_v1.Models;

public class Event
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string CategoryId { get; set; }
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
}