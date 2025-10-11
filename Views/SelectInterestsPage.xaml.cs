using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class SelectInterestsPage : ContentPage
{
    public SelectInterestsPage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        System.Diagnostics.Debug.WriteLine("🔄 SelectInterestsPage создана");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("👀 SelectInterestsPage появилась на экране");

        if (BindingContext is ProfileViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine($"📊 OnAppearing - AllInterests: {vm.AllInterests?.Count ?? 0}");

            // ПРИНУДИТЕЛЬНО ОБНОВЛЯЕМ ИНТЕРФЕЙС ПРИ ПОЯВЛЕНИИ СТРАНИЦЫ
            Device.BeginInvokeOnMainThread(() =>
            {
                vm.OnPropertyChanged(nameof(vm.AllInterests));
                vm.OnPropertyChanged(nameof(vm.SelectedInterests));
            });
        }
    }
}