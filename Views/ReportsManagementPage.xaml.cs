using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class ReportsManagementPage : ContentPage
{
    public ReportsManagementPage(ReportsManagementViewModel viewModel)
    {
        try
        {
            InitializeComponent();
            BindingContext = viewModel;
            System.Diagnostics.Debug.WriteLine("✅ ReportsManagementPage инициализирована");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка инициализации ReportsManagementPage: {ex.Message}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (BindingContext is ReportsManagementViewModel vm)
            {
                System.Diagnostics.Debug.WriteLine("🔄 OnAppearing: Загрузка жалоб...");
                await vm.LoadReports(); 
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в OnAppearing: {ex.Message}");
        }
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("///ModeratorDashboard");
            System.Diagnostics.Debug.WriteLine("✅ Возврат в админ-панель");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка возврата: {ex.Message}");
            await Shell.Current.GoToAsync("..");
        }
    }
}