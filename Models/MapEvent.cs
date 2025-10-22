namespace Point_v1.Models;

public class MapEvent
{
    public string EventId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int ParticipantsCount { get; set; }

    // Для Яндекс Карт используем простые double
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Вычисляемое свойство для отображения даты
    public string DateDisplay => EventDate.ToString("dd.MM.yyyy HH:mm");
}