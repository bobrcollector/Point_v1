using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class CreateEventPage : ContentPage
{
    public CreateEventPage(CreateEventViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

   
}