using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class EventDetailsPage : ContentPage
{
    public EventDetailsPage()
    {
        InitializeComponent();
    }

    // Query Property для получения eventId
    public static readonly BindableProperty EventIdProperty =
        BindableProperty.Create(nameof(EventId), typeof(string), typeof(EventDetailsPage), null,
        propertyChanged: OnEventIdChanged);

    public string EventId
    {
        get => (string)GetValue(EventIdProperty);
        set => SetValue(EventIdProperty, value);
    }

    public EventDetailsViewModel ViewModel => BindingContext as EventDetailsViewModel;

    private static void OnEventIdChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var page = (EventDetailsPage)bindable;
        page.LoadEventDetails();
    }

    private void LoadEventDetails()
    {
        if (ViewModel != null && !string.IsNullOrEmpty(EventId))
        {
            // Здесь можно обновить ViewModel с новым eventId
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel != null)
        {
            ViewModel.LoadEventDetailsCommand.Execute(null);
        }
    }
}