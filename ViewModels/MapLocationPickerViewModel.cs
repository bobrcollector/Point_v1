using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class MapLocationPickerViewModel : BaseViewModel
{
    private readonly IMapService _mapService;
    private string _mapHtmlContent = "";
    private double? _selectedLatitude;
    private double? _selectedLongitude;
    private string _selectedAddress = "";
    private bool _isLoading;
    private bool _isNavigating = false;

    public MapLocationPickerViewModel(IMapService mapService)
    {
        _mapService = mapService;
        ConfirmCommand = new Command(async () => await ConfirmSelection(), () => HasSelection);
        CancelCommand = new Command(async () => await Cancel());
        LoadMapCommand = new Command(async () => await LoadMap());
    }

    public string MapHtmlContent
    {
        get => _mapHtmlContent;
        set => SetProperty(ref _mapHtmlContent, value);
    }

    public double? SelectedLatitude
    {
        get => _selectedLatitude;
        set
        {
            SetProperty(ref _selectedLatitude, value);
            OnPropertyChanged(nameof(HasSelection));
            (ConfirmCommand as Command)?.ChangeCanExecute();
        }
    }

    public double? SelectedLongitude
    {
        get => _selectedLongitude;
        set
        {
            SetProperty(ref _selectedLongitude, value);
            OnPropertyChanged(nameof(HasSelection));
            (ConfirmCommand as Command)?.ChangeCanExecute();
        }
    }

    public string SelectedAddress
    {
        get => _selectedAddress;
        set => SetProperty(ref _selectedAddress, value);
    }

    public bool HasSelection => SelectedLatitude.HasValue && SelectedLongitude.HasValue;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand LoadMapCommand { get; }

    public event EventHandler<LocationSelectedEventArgs> LocationSelected;
    public event EventHandler Cancelled;

    private async Task LoadMap()
    {
        try
        {
            IsLoading = true;
            var location = await _mapService.GetCurrentLocationAsync();
            
            var mapHtmlService = new MapHtmlService();
            MapHtmlContent = mapHtmlService.GenerateLocationPickerMapHtml(
                location.Latitude, 
                location.Longitude,
                SelectedLatitude ?? location.Latitude,
                SelectedLongitude ?? location.Longitude
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки карты: {ex.Message}");
            var mapHtmlService = new MapHtmlService();
            MapHtmlContent = mapHtmlService.GenerateLocationPickerMapHtml(55.7558, 37.6173, 55.7558, 37.6173);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OnMapClick(double latitude, double longitude)
    {
        System.Diagnostics.Debug.WriteLine($"🗺️ OnMapClick вызван: lat={latitude}, lon={longitude}");
        SelectedLatitude = latitude;
        SelectedLongitude = longitude;
        
        System.Diagnostics.Debug.WriteLine($"✅ Координаты установлены. HasSelection: {HasSelection}");
        
        _ = GetAddressForCoordinates(latitude, longitude);
    }

    private async Task GetAddressForCoordinates(double latitude, double longitude)
    {
        try
        {
            var address = await _mapService.GetAddressFromCoordinatesAsync(latitude, longitude);
            SelectedAddress = address;
            System.Diagnostics.Debug.WriteLine($"📍 Адрес определен: {address}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения адреса: {ex.Message}");
            SelectedAddress = $"Широта: {latitude:F4}, Долгота: {longitude:F4}";
        }
    }

    private async Task ConfirmSelection()
    {
        if (_isNavigating)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Навигация уже выполняется, пропускаем повторный вызов");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"🔍 ConfirmSelection вызван. HasSelection: {HasSelection}, Lat: {SelectedLatitude}, Lon: {SelectedLongitude}");
        
        if (!HasSelection)
        {
            System.Diagnostics.Debug.WriteLine("❌ Нет выбранного местоположения");
            return;
        }

        _isNavigating = true;

        try
        {
            LocationSelectionService.SelectedLatitude = SelectedLatitude.Value;
            LocationSelectionService.SelectedLongitude = SelectedLongitude.Value;
            LocationSelectionService.SelectedAddress = SelectedAddress;
            
            System.Diagnostics.Debug.WriteLine($"📍 Сохранены координаты: lat={SelectedLatitude.Value}, lon={SelectedLongitude.Value}, address={SelectedAddress}");
            
            LocationSelected?.Invoke(this, new LocationSelectedEventArgs
            {
                Latitude = SelectedLatitude.Value,
                Longitude = SelectedLongitude.Value,
                Address = SelectedAddress
            });

            System.Diagnostics.Debug.WriteLine("🔄 Выполняем навигацию назад к CreateEventPage...");
            try
            {
                await Shell.Current.GoToAsync("//CreateEventPage");
                System.Diagnostics.Debug.WriteLine("✅ Навигация выполнена через Shell.GoToAsync(//CreateEventPage)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка навигации: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в ConfirmSelection: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            _isNavigating = false;
        }
    }

    private async Task Cancel()
    {
        if (_isNavigating)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Навигация уже выполняется, пропускаем Cancel");
            return;
        }

        System.Diagnostics.Debug.WriteLine("🔄 Cancel вызван");
        _isNavigating = true;

        try
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
            LocationSelectionService.Clear();
            System.Diagnostics.Debug.WriteLine("🔄 Выполняем навигацию назад к CreateEventPage (Cancel)...");
            await Shell.Current.GoToAsync("//CreateEventPage");
            System.Diagnostics.Debug.WriteLine("✅ Навигация выполнена (Cancel)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в Cancel: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            _isNavigating = false;
        }
    }
}

public class LocationSelectedEventArgs : EventArgs
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; }
}

