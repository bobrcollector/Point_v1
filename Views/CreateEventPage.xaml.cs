namespace Point_v1.Views;

public partial class CreateEventPage : ContentPage
{
    public CreateEventPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}