namespace Point_v1.Views;

public partial class TestInterestsPage : ContentPage
{
    public TestInterestsPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}