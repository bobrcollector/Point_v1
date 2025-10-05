using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HomeViewModel viewModel)
        {
            // Просто вызываем загрузку событий
            _ = viewModel.LoadEvents();
        }
    }
}