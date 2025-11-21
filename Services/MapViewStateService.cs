namespace Point_v1.Services;

public class MapViewStateService
{
    private bool _isMapViewActive = false;
    public bool IsMapViewActive
    {
        get => _isMapViewActive;
        set => _isMapViewActive = value;
    }

    public void SetMapViewActive(bool isActive)
    {
        _isMapViewActive = isActive;
        System.Diagnostics.Debug.WriteLine($"🗺️ MapViewStateService: IsMapViewActive = {_isMapViewActive}");
    }
}
