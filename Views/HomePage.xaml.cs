using Point_v1.ViewModels;
using Point_v1.Models;
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
            _ = viewModel.LoadEvents();
        }
    }

    // ОБЪЕДИНИ обработчики - оставь только этот метод
    private async void OnEventTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is Event eventItem)
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Тап по событию: {eventItem.Id} - {eventItem.Title}");

            if (BindingContext is HomeViewModel viewModel)
            {
                await viewModel.ViewEventDetails(eventItem.Id);
            }
        }
    }
}