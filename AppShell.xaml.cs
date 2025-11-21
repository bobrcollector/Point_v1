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
    private static readonly Color SelectedTabColor = Color.FromArgb("#4B0082");
    private static readonly Color LightUnselectedTabColor = Color.FromArgb("#6E6E6E");
    private static readonly Color DarkUnselectedTabColor = Color.FromArgb("#8E8E93");

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
            this.Navigated += OnShellNavigated;
            this.Loaded += OnShellLoaded;
            this.HandlerChanged += OnHandlerChanged;
            this.PropertyChanged += OnShellPropertyChanged;
            SetNavigationBarColor();
            SetTabBarColors();

            if (_authStateService != null)
            {
                _authStateService.AuthenticationStateChanged += OnAuthenticationStateChanged;
                _ = Task.Run(async () =>
                {
                    await Task.Delay(300);
                    MainThread.BeginInvokeOnMainThread(async () => await CheckAdminPermissions());
                });
            }
            else
            {
                AdminTab.IsVisible = false;
            }
        }
        catch (Exception)
        {
        }
    }

    private async void OnAuthenticationStateChanged(object sender, EventArgs e)
    {
        await CheckAdminPermissions();
    }

    private async Task CheckAdminPermissions()
    {
        try
        {
            if (_authStateService?.IsAuthenticated == true && _authorizationService != null)
            {
                var isModerator = await _authorizationService.IsModeratorAsync();
                AdminTab.IsVisible = isModerator;
            }
            else
            {
                AdminTab.IsVisible = false;
            }
        }
        catch (Exception)
        {
            AdminTab.IsVisible = false;
        }
    }

    private void OnShellLoaded(object sender, EventArgs e)
    {
        SetTabBarColors();
    }

    private void OnHandlerChanged(object sender, EventArgs e)
    {
        SetTabBarColors();
#if ANDROID
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(50);
            SetTabBarColors();
            await Task.Delay(100);
            SetTabBarColors();
        });
#endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_authStateService != null)
        {
            _authStateService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
        this.Navigated -= OnShellNavigated;
        this.Loaded -= OnShellLoaded;
        this.HandlerChanged -= OnHandlerChanged;
        this.PropertyChanged -= OnShellPropertyChanged;
    }
    
    private void SetNavigationBarColor()
    {
        try
        {
            Shell.SetTitleColor(this, Colors.White);
        }
        catch { }
    }
    
    private void SetTabBarColors()
    {
        try
        {
            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            var unselectedColor = isDark ? DarkUnselectedTabColor : LightUnselectedTabColor;
            Shell.SetTabBarForegroundColor(this, SelectedTabColor);
            Shell.SetTabBarUnselectedColor(this, unselectedColor);
            var tabBar = this.Items.OfType<TabBar>().FirstOrDefault();
            if (tabBar != null)
            {
                Shell.SetTabBarForegroundColor(tabBar, SelectedTabColor);
                Shell.SetTabBarUnselectedColor(tabBar, unselectedColor);
                Shell.SetTabBarForegroundColor(tabBar, SelectedTabColor);
                Shell.SetTabBarUnselectedColor(tabBar, unselectedColor);
                foreach (var tab in tabBar.Items.OfType<Tab>())
                {
                    Shell.SetTabBarForegroundColor(tab, SelectedTabColor);
                    Shell.SetTabBarUnselectedColor(tab, unselectedColor);
                    Shell.SetTabBarForegroundColor(tab, SelectedTabColor);
                    Shell.SetTabBarUnselectedColor(tab, unselectedColor);
                }
#if ANDROID
                SetAndroidTabBarColors();
#endif
#if WINDOWS
                SetWindowsTabBarColors();
#endif
            }
        }
        catch { }
    }

#if ANDROID
    private void SetAndroidTabBarColors()
    {
        try
        {
            var handler = this.Handler;
            if (handler?.PlatformView != null)
            {
                var platformView = handler.PlatformView as Android.Views.View;
                if (platformView != null)
                {
                    var bottomNavView = FindBottomNavigationView(platformView);
                    if (bottomNavView != null)
                    {
                        var selectedColor = Android.Graphics.Color.ParseColor("#4B0082");
                        var unselectedColor = Android.Graphics.Color.ParseColor("#6E6E6E");
                        var textColorStateList = CreateAndroidColorStateList(selectedColor, unselectedColor);
                        bottomNavView.ItemTextColor = textColorStateList;
                        var iconColorStateList = CreateAndroidColorStateList(selectedColor, unselectedColor);
                        bottomNavView.ItemIconTintList = iconColorStateList;
                        bottomNavView.RefreshDrawableState();
                    }
                }
            }
        }
        catch { }
    }

    private Google.Android.Material.BottomNavigation.BottomNavigationView FindBottomNavigationView(Android.Views.View view)
    {
        if (view is Google.Android.Material.BottomNavigation.BottomNavigationView bottomNav)
        {
            return bottomNav;
        }

        if (view is Android.Views.ViewGroup viewGroup)
        {
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                var child = viewGroup.GetChildAt(i);
                var result = FindBottomNavigationView(child);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }

    private Android.Content.Res.ColorStateList CreateAndroidColorStateList(Android.Graphics.Color selectedColor, Android.Graphics.Color unselectedColor)
    {
        var states = new int[][]
        {
            new int[] { Android.Resource.Attribute.StateChecked },
            new int[] { -Android.Resource.Attribute.StateChecked }
        };

        var colors = new int[]
        {
            selectedColor,
            unselectedColor
        };

        return new Android.Content.Res.ColorStateList(states, colors);
    }
#endif

#if WINDOWS
    private void SetWindowsTabBarColors()
    {
        try
        {
            var handler = this.Handler;
            if (handler?.PlatformView != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var tabBar = this.Items.OfType<TabBar>().FirstOrDefault();
                    if (tabBar != null)
                    {
                        Shell.SetTabBarForegroundColor(tabBar, SelectedTabColor);
                        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
                        var unselectedColor = isDark ? DarkUnselectedTabColor : LightUnselectedTabColor;
                        Shell.SetTabBarUnselectedColor(tabBar, unselectedColor);
                        foreach (var tab in tabBar.Items.OfType<Tab>())
                        {
                            Shell.SetTabBarForegroundColor(tab, SelectedTabColor);
                            Shell.SetTabBarUnselectedColor(tab, unselectedColor);
                        }
                    }
                });
            }
        }
        catch { }
    }
#endif
    
    private void OnShellPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(CurrentItem))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    SetTabBarColors();
                    await Task.Delay(50);
                    SetTabBarColors();
                    await Task.Delay(100);
                    SetTabBarColors();
                    await Task.Delay(150);
                    SetTabBarColors();
                });
            }
        }
        catch { }
    }
    
    private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
    {
        try
        {
            var currentPage = this.CurrentPage;
            if (currentPage != null)
            {
                if (!(currentPage is TabBar))
                {
                    Shell.SetBackgroundColor(currentPage, Color.FromArgb("#512BD4"));
                    Shell.SetTitleColor(currentPage, Colors.White);
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
            _ = CheckAdminPermissions();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                SetTabBarColors();
                await Task.Delay(50);
                SetTabBarColors();
                await Task.Delay(100);
                SetTabBarColors();
            });
        }
        catch { }
    }
}