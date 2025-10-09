namespace Point_v1.Services;

public class FilterStateService
{
    public string SearchText { get; set; } = "";
    public string SelectedCategory { get; set; } = "";
    public DateTime? SelectedDate { get; set; }
    public List<string> SelectedInterests { get; set; } = new List<string>();

    public bool HasActiveFilters =>
        !string.IsNullOrEmpty(SearchText) ||
        !string.IsNullOrEmpty(SelectedCategory) ||
        SelectedDate.HasValue ||
        SelectedInterests.Any();

    // НОВОЕ СВОЙСТВО: Список активных фильтров для отображения
    public List<string> ActiveFilterLabels
    {
        get
        {
            var labels = new List<string>();

            if (!string.IsNullOrEmpty(SearchText))
                labels.Add($"🔍 \"{SearchText}\"");

            if (!string.IsNullOrEmpty(SelectedCategory))
                labels.Add($"🏷️ {SelectedCategory}");

            if (SelectedDate.HasValue)
                labels.Add($"📅 {SelectedDate.Value:dd.MM.yyyy}");

            return labels;
        }
    }

    public void ClearFilters()
    {
        SearchText = "";
        SelectedCategory = "";
        SelectedDate = null;
        SelectedInterests.Clear();
    }
}