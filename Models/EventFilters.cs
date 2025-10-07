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
        (!string.IsNullOrEmpty(ParticipationStatus) && ParticipationStatus != "����� ������") ||
        (!string.IsNullOrEmpty(ParticipantCount) && ParticipantCount != "����� ����������") ||
        (!string.IsNullOrEmpty(SortOption) && SortOption != "�� ���� (������� �����)") ||
        SearchRadius != 10;
}