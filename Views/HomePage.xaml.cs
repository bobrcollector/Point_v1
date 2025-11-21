using Point_v1.ViewModels;
using Point_v1.Models;

namespace Point_v1.Views;

public partial class HomePage : ContentPage
{
    private HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        if (viewModel != null)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

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

                var htmlSource = new HtmlWebViewSource { Html = viewModel.MapHtmlContent };
                MapWebView.Source = htmlSource;
                System.Diagnostics.Debug.WriteLine("🗺️ WebView источник установлен");
            }
        }
        else if (e.PropertyName == nameof(HomeViewModel.IsMapView))
        {
            System.Diagnostics.Debug.WriteLine($"🎯 IsMapView изменен: {(BindingContext as HomeViewModel)?.IsMapView}");

            if (BindingContext is HomeViewModel viewModel)
            {
                if (viewModel.IsMapView && string.IsNullOrEmpty(viewModel.MapHtmlContent))
                {
                    _ = viewModel.LoadMapEvents();
                }
            }
        }
        else if (e.PropertyName == nameof(HomeViewModel.HasActiveFilters) || 
                 e.PropertyName == nameof(HomeViewModel.ActiveFilterLabels))
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Обновление фильтров: {e.PropertyName}");
            if (BindingContext is HomeViewModel viewModel)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 Панель фильтров обновлена: HasActiveFilters = {viewModel.HasActiveFilters}");
                });
            }
        }
    }

    private void OnMapNavigating(object sender, WebNavigatingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"🗺️ Навигация WebView: {e.Url}");

        if (e.Url.StartsWith("pointapp://event/"))
        {
            e.Cancel = true;
            var eventId = e.Url.Replace("pointapp://event/", "");
            System.Diagnostics.Debug.WriteLine($"🎯 Обработка клика по событию: {eventId}");

            if (!string.IsNullOrEmpty(eventId) && eventId != "undefined")
            {
                _ = OpenEventDetails(eventId);
            }
        }
        else if (e.Url.Contains("api-maps.yandex.ru") ||
                 e.Url.Contains("yastatic.net") ||
                 e.Url.Contains("yandex.net") ||
                 e.Url.Contains("mc.yandex.ru"))
        {
            System.Diagnostics.Debug.WriteLine("🗺️ Загрузка Яндекс ресурсов разрешена");
        }
        else if (e.Url == "about:blank" || e.Url.StartsWith("data:"))
        {
        }
        else if (e.Url.StartsWith("http") || e.Url.StartsWith("https"))
        {
            e.Cancel = true;
            System.Diagnostics.Debug.WriteLine($"🚫 Навигация заблокирована: {e.Url}");
        }
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

        try
        {
            if (BindingContext is HomeViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine("🗺️ OnAppearing: загружаем события");
                _ = viewModel.LoadEvents();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в OnAppearing: {ex.Message}");
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

    private async void OnCheckSelectedEventClicked(object sender, EventArgs e)
    {
        try
        {
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

    public async Task CenterMapOnEventAsync(string eventId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🗺️ CenterMapOnEventAsync вызван для события: {eventId}");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    await MapWebView.EvaluateJavaScriptAsync($"window.centerMapOnEvent('{eventId}')");
                    System.Diagnostics.Debug.WriteLine($"✅ Карта центрирована на событие: {eventId}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка при выполнении JavaScript: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка центрирования карты: {ex.Message}");
        }
    }

    public async Task CenterMapOnUserLocationAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🗺️ CenterMapOnUserLocationAsync вызван");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    await MapWebView.EvaluateJavaScriptAsync("window.centerOnUserLocation()");
                    System.Diagnostics.Debug.WriteLine("✅ Карта центрирована на местоположение пользователя");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка при выполнении JavaScript для centerOnUserLocation: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка центрирования карты на пользователя: {ex.Message}");
        }
    }
}