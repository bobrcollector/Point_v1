using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class FilterViewModel : BaseViewModel
{
    private readonly ISearchService _searchService;
    private readonly IMessagingService _messagingService;

    public FilterViewModel(ISearchService searchService, IMessagingService messagingService)
    {
        _searchService = searchService;
        _messagingService = messagingService;

        // Команды
        ResetFiltersCommand = new Command(ResetFilters);
        ApplyFiltersCommand = new Command(async () => await ApplyFilters());
        ClearDateCommand = new Command(ClearDate);
        SelectQuickDateCommand = new Command<QuickDateOption>(SelectQuickDate);

        // Загрузка данных
        LoadAvailableCategories();
        InitializeFilterOptions();

        ResetFilters();
    }

    // Команды
    public ICommand ResetFiltersCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearDateCommand { get; }
    public ICommand SelectQuickDateCommand { get; }

    // Свойства фильтров
    private string _selectedCategory;
    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
                UpdateActiveFilters();
        }
    }

    private DateTime? _selectedDate;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
                UpdateActiveFilters();
        }
    }

    private string _selectedParticipationStatus;
    public string SelectedParticipationStatus
    {
        get => _selectedParticipationStatus;
        set
        {
            if (SetProperty(ref _selectedParticipationStatus, value))
                UpdateActiveFilters();
        }
    }

    private string _selectedParticipantCount;
    public string SelectedParticipantCount
    {
        get => _selectedParticipantCount;
        set
        {
            if (SetProperty(ref _selectedParticipantCount, value))
                UpdateActiveFilters();
        }
    }

    private string _selectedSortOption;
    public string SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (SetProperty(ref _selectedSortOption, value))
                UpdateActiveFilters();
        }
    }

    private double _searchRadius = 10;
    public double SearchRadius
    {
        get => _searchRadius;
        set
        {
            if (SetProperty(ref _searchRadius, value))
                UpdateActiveFilters();
        }
    }

    // Коллекции для выбора
    private List<string> _availableCategories = new();
    public List<string> AvailableCategories
    {
        get => _availableCategories;
        set => SetProperty(ref _availableCategories, value);
    }

    private List<string> _participationStatuses = new()
    {
        "Любой статус",
        "Есть свободные места",
        "Я участвую",
        "Я не участвую"
    };
    public List<string> ParticipationStatuses
    {
        get => _participationStatuses;
        set => SetProperty(ref _participationStatuses, value);
    }

    private List<string> _participantCounts = new()
    {
        "Любое количество",
        "Мало участников (1-5)",
        "Средняя группа (6-15)",
        "Много участников (16+)"
    };
    public List<string> ParticipantCounts
    {
        get => _participantCounts;
        set => SetProperty(ref _participantCounts, value);
    }

    private List<string> _sortOptions = new()
    {
        "По дате (сначала новые)",
        "По дате (сначала старые)",
        "По количеству участников",
        "По расстоянию"
    };
    public List<string> SortOptions
    {
        get => _sortOptions;
        set => SetProperty(ref _sortOptions, value);
    }

    // Быстрые даты
    public List<QuickDateOption> QuickDates { get; } = new()
    {
        new QuickDateOption { Name = "Сегодня", Date = DateTime.Today },
        new QuickDateOption { Name = "Завтра", Date = DateTime.Today.AddDays(1) },
        new QuickDateOption { Name = "Выходные", Date = GetNextWeekend() },
        new QuickDateOption { Name = "Следующая неделя", Date = DateTime.Today.AddDays(7) }
    };

    // Активные фильтры для отображения
    private List<string> _activeFilters = new();
    public List<string> ActiveFilters
    {
        get => _activeFilters;
        set => SetProperty(ref _activeFilters, value);
    }

    private bool _hasActiveFilters;
    public bool HasActiveFilters
    {
        get => _hasActiveFilters;
        set => SetProperty(ref _hasActiveFilters, value);
    }

    // Методы
    private void ResetFilters()
    {
        SelectedCategory = null;
        SelectedDate = null;
        SelectedParticipationStatus = ParticipationStatuses[0];
        SelectedParticipantCount = ParticipantCounts[0];
        SelectedSortOption = SortOptions[0];
        SearchRadius = 10;

        System.Diagnostics.Debug.WriteLine("🧹 Фильтры сброшены");
    }

    private async Task ApplyFilters()
    {
        try
        {
            // Создаем объект фильтров
            var filters = new EventFilters
            {
                Category = SelectedCategory,
                Date = SelectedDate,
                ParticipationStatus = SelectedParticipationStatus,
                ParticipantCount = SelectedParticipantCount,
                SortOption = SelectedSortOption,
                SearchRadius = SearchRadius
            };

            System.Diagnostics.Debug.WriteLine("✅ Применяем фильтры:");
            System.Diagnostics.Debug.WriteLine($"   - Категория: {SelectedCategory}");
            System.Diagnostics.Debug.WriteLine($"   - Дата: {SelectedDate}");
            System.Diagnostics.Debug.WriteLine($"   - Статус: {SelectedParticipationStatus}");
            System.Diagnostics.Debug.WriteLine($"   - Участники: {SelectedParticipantCount}");
            System.Diagnostics.Debug.WriteLine($"   - Сортировка: {SelectedSortOption}");
            System.Diagnostics.Debug.WriteLine($"   - Радиус: {SearchRadius} км");

            // Отправляем сообщение с фильтрами
            _messagingService.Send("FiltersApplied", filters);

            // Возвращаемся на главную
            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка применения фильтров: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось применить фильтры", "OK");
        }
    }

    private void ClearDate()
    {
        SelectedDate = null;
    }

    private void SelectQuickDate(QuickDateOption quickDate)
    {
        SelectedDate = quickDate.Date;
    }

    private void UpdateActiveFilters()
    {
        var filters = new List<string>();

        if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "Все категории")
            filters.Add(SelectedCategory);

        if (SelectedDate.HasValue)
            filters.Add(SelectedDate.Value.ToString("dd.MM.yyyy"));

        if (!string.IsNullOrEmpty(SelectedParticipationStatus) && SelectedParticipationStatus != ParticipationStatuses[0])
            filters.Add(SelectedParticipationStatus);

        if (!string.IsNullOrEmpty(SelectedParticipantCount) && SelectedParticipantCount != ParticipantCounts[0])
            filters.Add(SelectedParticipantCount);

        if (!string.IsNullOrEmpty(SelectedSortOption) && SelectedSortOption != SortOptions[0])
            filters.Add(SelectedSortOption);

        if (SearchRadius != 10)
            filters.Add($"{SearchRadius} км");

        ActiveFilters = filters;
        HasActiveFilters = filters.Any();
    }

    private async void LoadAvailableCategories()
    {
        try
        {
            var categories = await _searchService.GetAvailableCategoriesAsync();
            categories.Insert(0, "Все категории");
            AvailableCategories = categories;
            System.Diagnostics.Debug.WriteLine($"✅ Загружено категорий: {AvailableCategories.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки категорий: {ex.Message}");
            AvailableCategories = new List<string> { "Все категории" };
        }
    }

    private void InitializeFilterOptions()
    {
        SelectedParticipationStatus = ParticipationStatuses[0];
        SelectedParticipantCount = ParticipantCounts[0];
        SelectedSortOption = SortOptions[0];
    }

    private static DateTime GetNextWeekend()
    {
        var today = DateTime.Today;
        var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        return today.AddDays(daysUntilSaturday);
    }
}

public class QuickDateOption
{
    public string Name { get; set; }
    public DateTime Date { get; set; }
}