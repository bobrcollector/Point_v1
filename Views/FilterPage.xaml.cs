using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class FilterPage : ContentPage
{
    public FilterPage(FilterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}