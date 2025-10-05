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

    // Вычисляемые свойства (не сохраняются в БД)
    public int ParticipantsCount => ParticipantIds?.Count ?? 0;
    public bool HasFreeSpots => ParticipantsCount < MaxParticipants;
    public string DateDisplay => EventDate.ToString("dd.MM.yyyy HH:mm");
    public string ShortDescription => Description?.Length > 100
        ? Description.Substring(0, 100) + "..."
        : Description;
}