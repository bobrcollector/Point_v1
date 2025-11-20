using Point_v1.Services;
using Point_v1.ViewModels;
using Point_v1.Views;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Point_v1;

public partial class AppShell : Shell
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthStateService _authStateService;

    // Конструктор по умолчанию для дизайнера
    public AppShell() : this(null, null)
    {
    }

    public AppShell(IAuthorizationService authorizationService, IAuthStateService authStateService)
    {
        try
        {
            InitializeComponent();

            _authorizationService = authorizationService;
            _authStateService = authStateService;
            
            // Подписываемся на событие навигации для установки цвета navigation bar
            this.Navigated += OnShellNavigated;
            
            // Настраиваем цвет navigation bar при инициализации
            SetNavigationBarColor();
            
            // Настраиваем цвета TabBar с небольшой задержкой для полной загрузки
            _ = Task.Run(async () =>
            {
                await Task.Delay(100);
                MainThread.BeginInvokeOnMainThread(() => SetTabBarColors());
            });

            System.Diagnostics.Debug.WriteLine($"========== 🔧 AppShell инициализируется ==========");
            System.Diagnostics.Debug.WriteLine($"  - IAuthorizationService: {(_authorizationService != null ? "✅" : "❌")}");
            System.Diagnostics.Debug.WriteLine($"  - IAuthStateService: {(_authStateService != null ? "✅" : "❌")}");

            // Проверяем, что сервисы не null
            if (_authStateService != null)
            {
                _authStateService.AuthenticationStateChanged += OnAuthenticationStateChanged;
                System.Diagnostics.Debug.WriteLine("✅ Обработчик AuthenticationStateChanged подписан");
                _ = CheckAdminPermissions();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ AuthStateService is null - админ-панель отключена");
                AdminTab.IsVisible = false;
            }
            System.Diagnostics.Debug.WriteLine($"========== ✅ AppShell инициализация завершена ==========");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в конструкторе AppShell: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        }
    }

    private async void OnAuthenticationStateChanged(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("========== 🔄 AuthenticationStateChanged событие вызвано ==========");
        await CheckAdminPermissions();
    }

    private async Task CheckAdminPermissions()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"========== 🔍 Проверка прав модератора ==========");
            System.Diagnostics.Debug.WriteLine($"  - IsAuthenticated: {_authStateService?.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"  - CurrentUserId: {_authStateService?.CurrentUserId}");
            System.Diagnostics.Debug.WriteLine($"  - AuthorizationService != null: {_authorizationService != null}");

            if (_authStateService?.IsAuthenticated == true && _authorizationService != null)
            {
                var isModerator = await _authorizationService.IsModeratorAsync();
                System.Diagnostics.Debug.WriteLine($"✅ Проверка прав завершена: модератор = {isModerator}");
                AdminTab.IsVisible = isModerator;
                System.Diagnostics.Debug.WriteLine($"🛡️ AdminTab.IsVisible = {AdminTab.IsVisible}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Пользователь не аутентифицирован или сервис отсутствует");
                AdminTab.IsVisible = false;
            }
            System.Diagnostics.Debug.WriteLine($"========== ✅ Проверка прав завершена ==========");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка проверки прав: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            AdminTab.IsVisible = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_authStateService != null)
        {
            _authStateService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
        this.Navigated -= OnShellNavigated;
    }
    
    private void SetNavigationBarColor()
    {
        try
        {
            // Устанавливаем цвет только для navigation bar (верхняя часть)
            Shell.SetForegroundColor(this, Colors.White);
            Shell.SetTitleColor(this, Colors.White);
            
            System.Diagnostics.Debug.WriteLine("✅ Цвет navigation bar установлен при инициализации");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка установки цвета navigation bar: {ex.Message}");
        }
    }
    
    private void SetTabBarColors()
    {
        try
        {
            // Находим TabBar и устанавливаем цвета для него
            var tabBar = this.Items.OfType<TabBar>().FirstOrDefault();
            if (tabBar != null)
            {
                var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
                
                // Устанавливаем цвета для TabBar - индиго (#4B0082) для выбранной вкладки
                Shell.SetForegroundColor(tabBar, Color.FromArgb("#4B0082")); // Индиго для выбранной вкладки
                Shell.SetUnselectedColor(tabBar, isDark ? Color.FromArgb("#8E8E93") : Color.FromArgb("#6E6E6E")); // Серый для невыбранных
                
                // Также устанавливаем цвета для каждой вкладки индивидуально
                foreach (var tab in tabBar.Items.OfType<Tab>())
                {
                    Shell.SetForegroundColor(tab, Color.FromArgb("#4B0082"));
                    Shell.SetUnselectedColor(tab, isDark ? Color.FromArgb("#8E8E93") : Color.FromArgb("#6E6E6E"));
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Цвета TabBar установлены: выбранная #512BD4, невыбранная {(isDark ? "#8E8E93" : "#6E6E6E")}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ TabBar не найден");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка установки цветов TabBar: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        }
    }
    
    private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
    {
        try
        {
            // Устанавливаем цвет navigation bar при каждой навигации (только верхняя часть)
            var currentPage = this.CurrentPage;
            if (currentPage != null)
            {
                // Устанавливаем цвет фона navigation bar только для страниц, которые не являются TabBar
                if (!(currentPage is TabBar))
                {
                    Shell.SetBackgroundColor(currentPage, Color.FromArgb("#512BD4"));
                    Shell.SetForegroundColor(currentPage, Colors.White);
                    Shell.SetTitleColor(currentPage, Colors.White);
                    
                    // Настраиваем кнопку "Назад" для страниц, которые должны её показывать
                    if (currentPage is FilterPage filterPage)
                    {
                        if (filterPage.BindingContext is FilterViewModel filterVm)
                        {
                            Shell.SetBackButtonBehavior(currentPage, new BackButtonBehavior
                            {
                                Command = filterVm.CloseCommand,
                                IsEnabled = true
                            });
                        }
                    }
                    else if (currentPage is ReportsManagementPage reportsPage)
                    {
                        if (reportsPage.BindingContext is ReportsManagementViewModel reportsVm)
                        {
                            Shell.SetBackButtonBehavior(currentPage, new BackButtonBehavior
                            {
                                Command = reportsVm.GoBackCommand,
                                IsEnabled = true
                            });
                        }
                    }
                }
            }
            
            // Обновляем цвета TabBar при навигации
            SetTabBarColors();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка установки цвета navigation bar при навигации: {ex.Message}");
        }
    }
}