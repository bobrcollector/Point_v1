namespace Point_v1.Models;

public class EventFilters
{
    public string Category { get; set; }
    public DateTime? Date { get; set; }
    public string ParticipationStatus { get; set; }
    public string ParticipantCount { get; set; }
    public string SortOption { get; set; }
    public double SearchRadius { get; set; }

    public bool HasActiveFilters =>
        !string.IsNullOrEmpty(Category) ||
        Date.HasValue ||
        (!string.IsNullOrEmpty(ParticipationStatus) && ParticipationStatus != "Любой статус") ||
        (!string.IsNullOrEmpty(ParticipantCount) && ParticipantCount != "Любое количество") ||
        (!string.IsNullOrEmpty(SortOption) && SortOption != "По дате (сначала новые)") ||
        SearchRadius != 10;
}