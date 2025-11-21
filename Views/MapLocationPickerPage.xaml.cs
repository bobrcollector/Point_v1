using Point_v1.ViewModels;

namespace Point_v1.Views;

[QueryProperty(nameof(InitialLat), "lat")]
[QueryProperty(nameof(InitialLon), "lon")]
public partial class MapLocationPickerPage : ContentPage
{
    public string InitialLat { get; set; }
    public string InitialLon { get; set; }

    public MapLocationPickerPage(MapLocationPickerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        viewModel.LocationSelected += OnLocationSelected;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MapLocationPickerViewModel vm)
        {
            if (double.TryParse(InitialLat, out double lat) && 
                double.TryParse(InitialLon, out double lon))
            {
                vm.SelectedLatitude = lat;
                vm.SelectedLongitude = lon;
            }
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MapLocationPickerViewModel.MapHtmlContent))
                {
                    LoadHtmlInWebView(vm.MapHtmlContent);
                }
            };
            
            vm.LoadMapCommand.Execute(null);
        }
    }

    private void LoadHtmlInWebView(string html)
    {
        if (string.IsNullOrEmpty(html)) return;
        
        try
        {
            var htmlSource = new HtmlWebViewSource { Html = html };
            MapWebView.Source = htmlSource;
            System.Diagnostics.Debug.WriteLine("✅ HTML загружен в WebView");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки HTML: {ex.Message}");
        }
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"🌐 WebView navigating to: {e.Url}");
        if (e.Url.StartsWith("app://location?"))
        {
            e.Cancel = true;
            System.Diagnostics.Debug.WriteLine($"📍 Перехвачен запрос выбора местоположения: {e.Url}");
            
            try
            {
                var uri = new Uri(e.Url);
                var query = uri.Query.TrimStart('?');
                var pairs = query.Split('&');
                
                double? lat = null;
                double? lon = null;
                
                foreach (var pair in pairs)
                {
                    var parts = pair.Split('=');
                    if (parts.Length == 2)
                    {
                        var key = parts[0];
                        var value = Uri.UnescapeDataString(parts[1]);
                        
                        if (key == "lat" && double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double latValue))
                            lat = latValue;
                        else if (key == "lon" && double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lonValue))
                            lon = lonValue;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"📍 Распарсенные координаты: lat={lat}, lon={lon}");
                
                if (lat.HasValue && lon.HasValue)
                {
                    if (BindingContext is MapLocationPickerViewModel vm)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Вызываем OnMapClick с координатами: {lat.Value}, {lon.Value}");
                        vm.OnMapClick(lat.Value, lon.Value);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ BindingContext не является MapLocationPickerViewModel");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Не удалось распарсить координаты");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка обработки координат: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }
    }

    private void OnLocationSelected(object sender, LocationSelectedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"✅ Местоположение выбрано: {e.Latitude}, {e.Longitude}");
    }
}

