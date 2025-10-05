using Microsoft.Extensions.Logging;
using Point_v1.Services;
using Point_v1.ViewModels;
using Point_v1.Views;

namespace Point_v1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Регистрируем сервисы
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IAuthStateService, AuthStateService>();
        builder.Services.AddSingleton<IDataService, DataService>();
        builder.Services.AddSingleton<ISearchService, SearchService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();


        // Регистрируем ViewModels
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<CreateEventViewModel>();
        builder.Services.AddTransient<FilterViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Регистрируем Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<CreateEventPage>();
        builder.Services.AddTransient<FilterPage>();
        builder.Services.AddTransient<MyEventsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<RegisterPage>();

        // Регистрируем конвертеры
        builder.Services.AddSingleton<Converters.InverseBoolConverter>();
        builder.Services.AddSingleton<Converters.StringNotEmptyConverter>();
        builder.Services.AddSingleton<Converters.IsNotNullOrEmptyConverter>();

        // AppShell
        builder.Services.AddSingleton<AppShell>();


        return builder.Build();
    }
}