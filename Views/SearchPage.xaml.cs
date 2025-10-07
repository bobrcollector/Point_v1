using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class SearchPage : ContentPage
{
    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }
}