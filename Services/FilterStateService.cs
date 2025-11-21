namespace Point_v1.Services;

public class FilterStateService
{
    private string _searchText = "";
    private string _selectedCategory = "";
    private DateTime? _selectedDate;
    private List<string> _selectedInterests = new List<string>();
    private List<string> _cachedFilterLabels = new List<string>();

    public event EventHandler FiltersChanged;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnFiltersChanged();
            }
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnFiltersChanged();
            }
        }
    }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate != value)
            {
                _selectedDate = value;
                OnFiltersChanged();
            }
        }
    }

    public List<string> SelectedInterests
    {
        get => _selectedInterests;
        set => _selectedInterests = value;
    }

    public bool HasActiveFilters =>
        !string.IsNullOrEmpty(SearchText) ||
        !string.IsNullOrEmpty(SelectedCategory) ||
        SelectedDate.HasValue ||
        SelectedInterests.Any();

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

            _cachedFilterLabels = labels;
            System.Diagnostics.Debug.WriteLine($"🎯 ActiveFilterLabels обновлены: {string.Join(", ", labels)}");
            
            return labels;
        }
    }

    public void ClearFilters()
    {
        _searchText = "";
        _selectedCategory = "";
        _selectedDate = null;
        _selectedInterests.Clear();
        _cachedFilterLabels.Clear();
        OnFiltersChanged();
    }
    private void OnFiltersChanged()
    {
        System.Diagnostics.Debug.WriteLine($"🎯 FiltersChanged вызван: SearchText='{_searchText}', Category='{_selectedCategory}', Date={_selectedDate}, HasActiveFilters={HasActiveFilters}");
        FiltersChanged?.Invoke(this, EventArgs.Empty);
    }
}
