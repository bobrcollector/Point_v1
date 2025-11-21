using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class FilterViewModel : BaseViewModel
{
    private readonly FilterStateService _filterStateService;
    private readonly ISearchService _searchService;
    private readonly MapViewStateService _mapViewStateService;

    public FilterViewModel(FilterStateService filterStateService, ISearchService searchService, MapViewStateService mapViewStateService)
    {
        _filterStateService = filterStateService;
        _searchService = searchService;
        _mapViewStateService = mapViewStateService;

        ApplyFiltersCommand = new Command(async () => await ApplyFilters());
        ResetFiltersCommand = new Command(async () => await ResetFilters());
        CloseCommand = new Command(async () => await Close());

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

    private bool _wasMapViewActive = false;
    public bool WasMapViewActive
    {
        get => _wasMapViewActive;
        set => SetProperty(ref _wasMapViewActive, value);
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

            _filterStateService.SearchText = SearchText;
            _filterStateService.SelectedCategory = SelectedCategory;
            _filterStateService.SelectedDate = SelectedDate;

            System.Diagnostics.Debug.WriteLine($"✅ Фильтры сохранены, IsMapViewActive = {_mapViewStateService.IsMapViewActive}");
            
            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка применения фильтров: {ex.Message}");
            try
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось применить фильтры", "OK");
            }
            catch { }
        }
    }

    private async Task ResetFilters()
    {
        try
        {
            SearchText = "";
            SelectedCategory = "";
            SelectedDate = null;

            _filterStateService.ClearFilters();

            System.Diagnostics.Debug.WriteLine($"✅ Фильтры очищены, IsMapViewActive = {_mapViewStateService.IsMapViewActive}");
            
            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка сброса фильтров: {ex.Message}");
            try
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сбросить фильтры", "OK");
            }
            catch { }
        }
    }

    private async Task Close()
    {
        System.Diagnostics.Debug.WriteLine($"🔚 Закрытие страницы фильтров без применения, IsMapViewActive = {_mapViewStateService.IsMapViewActive}");
        await Shell.Current.GoToAsync("//HomePage");
    }
}