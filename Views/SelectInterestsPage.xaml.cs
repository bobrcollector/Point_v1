using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class SelectInterestsPage : ContentPage
{
    public SelectInterestsPage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        System.Diagnostics.Debug.WriteLine("🔄 SelectInterestsPage создана");
        if (BindingContext is ProfileViewModel vm)
        {
            vm.CopyToTempData();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("👀 SelectInterestsPage появилась на экране");

        if (BindingContext is ProfileViewModel vm)
        {
            if (vm.TempAllInterests?.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("🔄 Временные данные пустые, загружаем...");
                await vm.LoadInterestsForSelection();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"✅ Временные данные уже есть: {vm.TempAllInterests.Count} интересов");
                Device.BeginInvokeOnMainThread(() =>
                {
                    vm.OnPropertyChanged(nameof(vm.TempAllInterests));
                    vm.OnPropertyChanged(nameof(vm.TempSelectedInterests));
                });
            }
        }
    }
}