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
        builder.Services.AddSingleton<ISearchService, SearchService>();
        builder.Services.AddSingleton<IMessagingService, MessagingService>();



        // Регистрируем ViewModels
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<CreateEventViewModel>();
        builder.Services.AddTransient<FilterViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<EventDetailsViewModel>();
        builder.Services.AddTransient<SearchViewModel>();

        // Регистрируем Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<CreateEventPage>();
        builder.Services.AddTransient<FilterPage>();
        builder.Services.AddTransient<MyEventsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<EventDetailsPage>();
        builder.Services.AddTransient<SearchPage>();
        // Добавьте эти строки в метод ConfigureServices:

        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<FilterViewModel>();


        // Регистрируем конвертеры
        builder.Services.AddSingleton<Converters.InverseBoolConverter>();
        builder.Services.AddSingleton<Converters.StringNotEmptyConverter>();
        builder.Services.AddSingleton<Converters.IsNotNullOrEmptyConverter>();
        builder.Services.AddSingleton<Converters.InverseBoolConverter>();



        Routing.RegisterRoute(nameof(EventDetailsPage), typeof(EventDetailsPage));

        builder.Services.AddSingleton<AppShell>();


        return builder.Build();
    }
}