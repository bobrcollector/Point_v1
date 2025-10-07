using Point_v1.ViewModels;
using Point_v1.Services;

namespace Point_v1.Views;

public partial class EventDetailsPage : ContentPage
{
    public EventDetailsPage(EventDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is EventDetailsViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.EventId) &&
                !string.IsNullOrEmpty(GlobalEventId.EventId))
            {
                viewModel.EventId = GlobalEventId.EventId;
                GlobalEventId.EventId = null;
            }
        }
    }
}