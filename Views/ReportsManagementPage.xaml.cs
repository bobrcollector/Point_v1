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
            // Устанавливаем цвет navigation bar для этой страницы
            Shell.SetBackgroundColor(this, Color.FromArgb("#512BD4"));
            Shell.SetForegroundColor(this, Colors.White);
            Shell.SetTitleColor(this, Colors.White);
            
            if (BindingContext is ReportsManagementViewModel vm)
            {
                // Настраиваем стандартную кнопку "Назад" через Shell
                // Shell автоматически покажет кнопку "Назад", если есть предыдущая страница в стеке
                // Если нет, используем кастомную команду
                Shell.SetBackButtonBehavior(this, new BackButtonBehavior
                {
                    Command = vm.GoBackCommand,
                    IsEnabled = true,
                    IconOverride = null // Используем стандартную иконку стрелки
                });
                
                System.Diagnostics.Debug.WriteLine("🔄 OnAppearing: Загрузка жалоб...");
                await vm.LoadReports(); 
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в OnAppearing: {ex.Message}");
        }
    }

}