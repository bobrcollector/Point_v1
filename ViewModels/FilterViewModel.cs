using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class DateOption
{
    public string DisplayName { get; set; }
    public DateTime? Date { get; set; }
}

public class SortOption
{
    public string DisplayName { get; set; }
    public string SortBy { get; set; }
}

public class InterestWithSelection : Interest
{
    public bool IsSelected { get; set; }
}

public class FilterViewModel : BaseViewModel
{
    public FilterViewModel()
    {
        ApplyFiltersCommand = new Command(async () => await ApplyFilters());
        ResetFiltersCommand = new Command(async () => await ResetFilters());
    }

    public ICommand ApplyFiltersCommand { get; }
    public ICommand ResetFiltersCommand { get; }

    private async Task ApplyFilters()
    {
        // ��������� ������
        await Application.Current.MainPage.DisplayAlert("�������", "������� ���������", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async Task ResetFilters()
    {
        // ��������� ������
        await Application.Current.MainPage.DisplayAlert("�������", "������� ��������", "OK");
    }
}