using Point_v1.Views;

namespace Point_v1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Используем Shell для навигации
        MainPage = new AppShell();
    }
}