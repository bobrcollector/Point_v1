using Point_v1.ViewModels;
using Point_v1.Models;

namespace Point_v1.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Подписываемся на изменение HTML контента
        if (viewModel != null)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        // Обработчик сообщений от WebView
        MapWebView.Navigating += OnMapNavigating;
        MapWebView.Navigated += OnMapNavigated;
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"🎯 PropertyChanged: {e.PropertyName}");

        if (e.PropertyName == nameof(HomeViewModel.MapHtmlContent))
        {
            if (BindingContext is HomeViewModel viewModel && !string.IsNullOrEmpty(viewModel.MapHtmlContent))
            {
                System.Diagnostics.Debug.WriteLine("🗺️ Загружаем HTML в WebView");

                // Загружаем HTML в WebView
                var htmlSource = new HtmlWebViewSource { Html = viewModel.MapHtmlContent };
                MapWebView.Source = htmlSource;
                System.Diagnostics.Debug.WriteLine("🗺️ WebView источник установлен");
            }
        }
        else if (e.PropertyName == nameof(HomeViewModel.IsMapView))
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Режим изменен: {(e.PropertyName == nameof(HomeViewModel.IsMapView) ? "Карта" : "Список")}");

            if (BindingContext is HomeViewModel viewModel)
            {
                if (viewModel.IsMapView && string.IsNullOrEmpty(viewModel.MapHtmlContent))
                {
                    // Если переключились на карту, но контент пустой - загружаем
                    _ = viewModel.LoadMapEvents();
                }
            }
        }
    }

    private void OnMapNavigating(object sender, WebNavigatingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"🗺️ Навигация WebView: {e.Url}");

        // Обрабатываем специальные URL для взаимодействия с картой
        if (e.Url.StartsWith("pointapp://event/"))
        {
            e.Cancel = true; // Отменяем стандартную навигацию

            var eventId = e.Url.Replace("pointapp://event/", "");
            System.Diagnostics.Debug.WriteLine($"🎯 Обработка клика по событию: {eventId}");

            if (!string.IsNullOrEmpty(eventId) && eventId != "undefined")
            {
                // Переходим к деталям события
                _ = OpenEventDetails(eventId);
            }
        }
        else if (e.Url.Contains("api-maps.yandex.ru") ||
                 e.Url.Contains("yastatic.net") ||
                 e.Url.Contains("yandex.net") ||
                 e.Url.Contains("mc.yandex.ru"))
        {
            // Разрешаем загрузку всех ресурсов Яндекс
            System.Diagnostics.Debug.WriteLine("🗺️ Загрузка Яндекс ресурсов разрешена");
        }
        else if (e.Url == "about:blank" || e.Url.StartsWith("data:"))
        {
            // Разрешаем пустые URL и data URI
        }
        else if (e.Url.StartsWith("http") || e.Url.StartsWith("https"))
        {
            // Блокируем все остальные HTTP/HTTPS URL
            e.Cancel = true;
            System.Diagnostics.Debug.WriteLine($"🚫 Навигация заблокирована: {e.Url}");
        }
        // Все остальные URL (относительные пути и т.д.) разрешаем
    }

    private async void OnReloadMapClicked(object sender, EventArgs e)
    {
        if (BindingContext is HomeViewModel viewModel && viewModel.IsMapView)
        {
            System.Diagnostics.Debug.WriteLine("🔄 Принудительная перезагрузка карты");
            await viewModel.LoadMapEvents();
        }
    }

    private void OnMapNavigated(object sender, WebNavigatedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"🗺️ WebView загружен: {e.Url}, Результат: {e.Result}");

        if (e.Result != WebNavigationResult.Success)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки WebView: {e.Result}");
        }
    }

    private async Task OpenEventDetails(string eventId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Переход к событию: {eventId}");

            if (BindingContext is HomeViewModel viewModel)
            {
                await viewModel.ViewEventDetails(eventId);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка перехода к событию: {ex.Message}");
            await DisplayAlert("Ошибка", "Не удалось открыть событие", "OK");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HomeViewModel viewModel)
        {
            _ = viewModel.LoadEvents();
        }
    }

    private async void OnEventTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is Event eventItem)
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Тап по событию в списке: {eventItem.Id} - {eventItem.Title}");

            if (BindingContext is HomeViewModel viewModel)
            {
                await viewModel.ViewEventDetails(eventItem.Id);
            }
        }
    }

    // ДОБАВЬ метод для проверки выбранного события (для отладки)
    private async void OnCheckSelectedEventClicked(object sender, EventArgs e)
    {
        try
        {
            // Этот метод можно вызвать для тестирования
            var result = await MapWebView.EvaluateJavaScriptAsync("window.getSelectedEventId()");
            System.Diagnostics.Debug.WriteLine($"🔍 Выбранное событие: {result}");

            if (!string.IsNullOrEmpty(result) && result != "null")
            {
                await OpenEventDetails(result);
            }
            else
            {
                await DisplayAlert("Инфо", "Событие не выбрано", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка проверки события: {ex.Message}");
        }
    }
}