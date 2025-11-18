using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class ModeratorDashboardPage : ContentPage
{
    public ModeratorDashboardPage(ModeratorDashboardViewModel viewModel)
    {
        try
        {
            InitializeComponent();
            BindingContext = viewModel;
            System.Diagnostics.Debug.WriteLine("✅ ModeratorDashboardPage инициализирована");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка инициализации ModeratorDashboardPage: {ex.Message}");
        }
    }

    private async void OnAllReportsClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 Переход к управлению жалобами...");
            await Shell.Current.GoToAsync("//ReportsManagementPage");
            System.Diagnostics.Debug.WriteLine("✅ Успешный переход к ReportsManagementPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка перехода: {ex.Message}");
            await DisplayAlert("Ошибка", "Не удалось открыть страницу жалоб", "OK");
        }
    }
}