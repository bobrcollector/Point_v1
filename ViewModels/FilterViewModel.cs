using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class FilterViewModel : BaseViewModel
{
    private readonly FilterStateService _filterStateService;
    private readonly ISearchService _searchService;

    public FilterViewModel(FilterStateService filterStateService, ISearchService searchService)
    {
        _filterStateService = filterStateService;
        _searchService = searchService;

        ApplyFiltersCommand = new Command(async () => await ApplyFilters());
        ResetFiltersCommand = new Command(async () => await ResetFilters());
        CloseCommand = new Command(async () => await Close());

        // Загружаем текущие значения фильтров
        LoadCurrentFilters();
        LoadAvailableCategories();
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

    private List<string> _availableCategories = new List<string>();
    public List<string> AvailableCategories
    {
        get => _availableCategories;
        set => SetProperty(ref _availableCategories, value);
    }

    public ICommand ApplyFiltersCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    public ICommand CloseCommand { get; }

    private void LoadCurrentFilters()
    {
        SearchText = _filterStateService.SearchText;
        SelectedCategory = _filterStateService.SelectedCategory;
        SelectedDate = _filterStateService.SelectedDate;
    }

    private async void LoadAvailableCategories()
    {
        try
        {
            AvailableCategories = await _searchService.GetAvailableCategoriesAsync();
            System.Diagnostics.Debug.WriteLine($"✅ Загружено категорий для фильтров: {AvailableCategories.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки категорий: {ex.Message}");
        }
    }

    private async Task ApplyFilters()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Применяем фильтры: '{SearchText}', '{SelectedCategory}', {SelectedDate}");

            // Сохраняем фильтры в сервис
            _filterStateService.SearchText = SearchText;
            _filterStateService.SelectedCategory = SelectedCategory;
            _filterStateService.SelectedDate = SelectedDate;

            await Application.Current.MainPage.DisplayAlert("Фильтры", "Фильтры применены", "OK");
            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка применения фильтров: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось применить фильтры", "OK");
        }
    }

    private async Task ResetFilters()
    {
        SearchText = "";
        SelectedCategory = "";
        SelectedDate = null;

        // Очищаем фильтры в сервисе
        _filterStateService.ClearFilters();

        await Application.Current.MainPage.DisplayAlert("Фильтры", "Фильтры сброшены", "OK");
        await Shell.Current.GoToAsync("//HomePage");
    }

    private async Task Close()
    {
        await Shell.Current.GoToAsync("//HomePage");
    }
}