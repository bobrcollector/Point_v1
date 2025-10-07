using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class SearchViewModel : BaseViewModel
{
    private readonly ISearchService _searchService;

    public SearchViewModel(ISearchService searchService)
    {
        _searchService = searchService;

        SearchCommand = new Command(async () => await PerformSearch());
        ClearFiltersCommand = new Command(async () => await ClearFilters());
        ApplyFiltersCommand = new Command(async () => await ApplyFilters());

        // ПРАВИЛЬНАЯ РЕАЛИЗАЦИЯ КОМАНДЫ НАЗАД
        GoBackCommand = new Command(async () => await GoBack());

        // Загружаем доступные категории
        LoadAvailableCategories();
    }

    public ICommand GoBackCommand { get; }

    private async Task GoBack()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔙 Нажата кнопка назад");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка навигации назад: {ex.Message}");
        }
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedCategory = "";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    private DateTime? _selectedDate;
    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    private List<Event> _searchResults = new List<Event>();
    public List<Event> SearchResults
    {
        get => _searchResults;
        set => SetProperty(ref _searchResults, value);
    }

    private List<string> _availableCategories = new List<string>();
    public List<string> AvailableCategories
    {
        get => _availableCategories;
        set => SetProperty(ref _availableCategories, value);
    }

    private bool _hasSearchResults;
    public bool HasSearchResults
    {
        get => _hasSearchResults;
        set => SetProperty(ref _hasSearchResults, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ApplyFiltersCommand { get; }

    private async Task PerformSearch()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Выполняется поиск: '{SearchText}', категория: '{SelectedCategory}', дата: {SelectedDate}");

            var results = await _searchService.SearchEventsAsync(SearchText, SelectedCategory, SelectedDate);
            SearchResults = results;
            HasSearchResults = results.Any();

            System.Diagnostics.Debug.WriteLine($"✅ Найдено событий: {results.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка поиска: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось выполнить поиск", "OK");
        }
    }

    private async Task ClearFilters()
    {
        SearchText = "";
        SelectedCategory = "";
        SelectedDate = null;
        SearchResults = new List<Event>();
        HasSearchResults = false;

        System.Diagnostics.Debug.WriteLine("🧹 Фильтры очищены");
    }

    private async Task ApplyFilters()
    {
        await PerformSearch();
    }

    private async void LoadAvailableCategories()
    {
        try
        {
            AvailableCategories = await _searchService.GetAvailableCategoriesAsync();
            System.Diagnostics.Debug.WriteLine($"✅ Загружено категорий: {AvailableCategories.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки категорий: {ex.Message}");
        }
    }

}