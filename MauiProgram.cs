using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Point_v1.Converters;
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
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        RegisterServices(builder);
        RegisterViewModels(builder);
        RegisterPages(builder);
        RegisterConverters(builder);
        RegisterRoutes();

        return builder.Build();
    }

    private static void RegisterServices(MauiAppBuilder builder)
    {
        // Основные сервисы
        builder.Services.AddSingleton<IAuthService, FirebaseAuthService>();
        builder.Services.AddSingleton<IDataService, FirestoreDataService>();
        builder.Services.AddSingleton<IAuthStateService, AuthStateService>();
        builder.Services.AddSingleton<ISearchService, SearchService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IMessagingService, MessagingService>();
        builder.Services.AddSingleton<IMapService, MapService>();
        builder.Services.AddSingleton<MapHtmlService>();
        builder.Services.AddSingleton<IAuthorizationService, AuthorizationService>();
        builder.Services.AddSingleton<IReportService, ReportService>();
        builder.Services.AddSingleton<NavigationStateService>();
        // Вспомогательные сервисы
        builder.Services.AddSingleton<FilterStateService>();
        builder.Services.AddSingleton<MapViewStateService>();
    }

    private static void RegisterViewModels(MauiAppBuilder builder)
    {
        // Основные ViewModels
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<CreateEventViewModel>();
        builder.Services.AddTransient<FilterViewModel>();
        builder.Services.AddTransient<EventDetailsViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<MyEventsViewModel>();
        builder.Services.AddTransient<ModeratorDashboardViewModel>();
        builder.Services.AddTransient<ReportsManagementViewModel>();
        builder.Services.AddTransient<SelectInterestsViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ModeratorDashboardViewModel>();
        builder.Services.AddTransient<ReportsManagementViewModel>();
    }

    private static void RegisterPages(MauiAppBuilder builder)
    {
        // Основные страницы
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<CreateEventPage>();
        builder.Services.AddTransient<FilterPage>();
        builder.Services.AddTransient<MyEventsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<EventDetailsPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ModeratorDashboardPage>();
        builder.Services.AddTransient<ReportsManagementPage>();
        builder.Services.AddTransient<EditProfilePage>();
        builder.Services.AddTransient<SelectInterestsPage>();
        builder.Services.AddTransient<TestInterestsPage>();
        builder.Services.AddTransient<ModeratorDashboardPage>();
        builder.Services.AddTransient<ReportsManagementPage>();

        // AppShell
        builder.Services.AddSingleton<AppShell>();
    }

    private static void RegisterConverters(MauiAppBuilder builder)
    {
        // Конвертеры для интересов
        builder.Services.AddSingleton<InterestSelectionToColorConverter>();
        builder.Services.AddSingleton<InterestSelectionToTextColorConverter>();

        // Базовые конвертеры
        builder.Services.AddSingleton<BoolToBorderColorConverter>();
        builder.Services.AddSingleton<BoolToColorConverter>();
        builder.Services.AddSingleton<BoolToTextColorConverter>();
        builder.Services.AddSingleton<BoolToSymbolConverter>();

        // Конвертеры для логики
        builder.Services.AddSingleton<InverseBoolConverter>();
        builder.Services.AddSingleton<StringNotEmptyConverter>();
        builder.Services.AddSingleton<IsNotNullOrEmptyConverter>();
        builder.Services.AddSingleton<IsNotStringConverter>();
        builder.Services.AddSingleton<IsNotNullConverter>();

        // Конвертеры для отчетов
        builder.Services.AddSingleton<ShowReportActionsConverter>();
        builder.Services.AddSingleton<ShowResolutionInfoConverter>();
        builder.Services.AddSingleton<TabColorConverter>();
        builder.Services.AddSingleton<TabTextColorConverter>();

        builder.Services.AddSingleton<ShowReportActionsConverter>();
        builder.Services.AddSingleton<ShowResolutionInfoConverter>();
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(EventDetailsPage), typeof(EventDetailsPage));
        Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
        Routing.RegisterRoute(nameof(SelectInterestsPage), typeof(SelectInterestsPage));
        Routing.RegisterRoute(nameof(TestInterestsPage), typeof(TestInterestsPage));
    }
}