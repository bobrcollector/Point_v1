using Point_v1.ViewModels;
using Point_v1.Services;

namespace Point_v1.Views;

[QueryProperty(nameof(EventId), "eventId")]
public partial class EventDetailsPage : ContentPage
{
    public EventDetailsPage(EventDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public string EventId
    {
        set
        {
            if (BindingContext is EventDetailsViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"🎯 Query Property получен: {value}");
                viewModel.EventId = value;
                _ = viewModel.LoadEventDetails();
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is EventDetailsViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.EventId))
            {
                await Task.Delay(100);

                if (string.IsNullOrEmpty(viewModel.EventId))
                {
                    System.Diagnostics.Debug.WriteLine("❌ EventId не установлен после OnAppearing");
                    await DisplayAlert("Ошибка", "Событие не найдено", "OK");
                    await GoBack();
                }
            }
        }
    }

    private async Task GoBack()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            await Shell.Current.GoToAsync("///HomePage");
        }
    }
}