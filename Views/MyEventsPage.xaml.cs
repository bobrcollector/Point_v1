using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class MyEventsPage : ContentPage
{
    public MyEventsPage(MyEventsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MyEventsViewModel viewModel)
        {
            _ = viewModel.LoadEvents();
        }
    }
}