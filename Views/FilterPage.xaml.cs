using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class FilterPage : ContentPage
{
    public FilterPage(FilterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        if (viewModel != null)
        {
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                Command = viewModel.CloseCommand,
                IsEnabled = true
            });
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            Shell.SetBackgroundColor(this, Color.FromArgb("#512BD4"));
            Shell.SetForegroundColor(this, Colors.White);
            Shell.SetTitleColor(this, Colors.White);
            if (BindingContext is FilterViewModel vm)
            {
                Shell.SetBackButtonBehavior(this, new BackButtonBehavior
                {
                    Command = vm.CloseCommand,
                    IsEnabled = true
                });
                
                System.Diagnostics.Debug.WriteLine($"✅ FilterPage: BackButtonBehavior настроен повторно в OnAppearing");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в OnAppearing FilterPage: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        }
    }
}