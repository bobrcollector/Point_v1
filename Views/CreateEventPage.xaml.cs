using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class CreateEventPage : ContentPage
{
    public CreateEventPage(CreateEventViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Можно добавить дополнительную инициализацию если нужно
        if (BindingContext is CreateEventViewModel viewModel)
        {
            // Автоматическая загрузка интересов при появлении страницы
        }
    }
}