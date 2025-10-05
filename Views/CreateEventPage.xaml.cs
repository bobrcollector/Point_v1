using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class CreateEventPage : ContentPage
{
    public CreateEventPage(CreateEventViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("🔄 Кнопка Отмена нажата (Clicked)");
        await Shell.Current.GoToAsync("//HomePage");
    }
}