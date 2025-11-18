namespace Point_v1.Services;

public static class LocationSelectionService
{
    public static double? SelectedLatitude { get; set; }
    public static double? SelectedLongitude { get; set; }
    public static string SelectedAddress { get; set; }
    public static bool HasSelection => SelectedLatitude.HasValue && SelectedLongitude.HasValue;

    public static void Clear()
    {
        SelectedLatitude = null;
        SelectedLongitude = null;
        SelectedAddress = null;
    }
}

