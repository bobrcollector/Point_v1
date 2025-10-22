using Microsoft.Maui.Platform;
using Point_v1.Models;
using Point_v1.ViewModels;

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
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HomeViewModel viewModel)
        {
            _ = viewModel.LoadEvents();
        }
    }

    private void OnMapNavigating(object sender, WebNavigatingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"🗺️ Загрузка карты: {e.Url}");
    }

    private async void OnEventTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is Event eventItem)
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Тап по событию: {eventItem.Id} - {eventItem.Title}");

            if (BindingContext is HomeViewModel viewModel)
            {
                await viewModel.ViewEventDetails(eventItem.Id);
            }
        }
    }
}