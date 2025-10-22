using Point_v1.Models;

namespace Point_v1.Services;

public class MapService : IMapService
{
    private readonly IDataService _dataService;

    public MapService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<Location> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium);
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
                return location;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения локации: {ex.Message}");
        }

        return new Location(55.7558, 37.6173); // Москва по умолчанию
    }

    public async Task<List<MapEvent>> GetEventsNearbyAsync(Location center, double radiusKm = 5)
    {
        try
        {
            var events = await _dataService.GetEventsAsync();
            var nearbyEvents = new List<MapEvent>();

            if (events == null) return nearbyEvents;

            foreach (var eventItem in events)
            {
                if (eventItem.Latitude.HasValue && eventItem.Longitude.HasValue)
                {
                    var mapEvent = new MapEvent
                    {
                        EventId = eventItem.Id,
                        Title = eventItem.Title,
                        Description = eventItem.ShortDescription,
                        Address = eventItem.Address,
                        CategoryId = eventItem.CategoryId,
                        EventDate = eventItem.EventDate,
                        ParticipantsCount = eventItem.ParticipantsCount,
                        Latitude = eventItem.Latitude.Value,
                        Longitude = eventItem.Longitude.Value
                    };
                    nearbyEvents.Add(mapEvent);
                }
            }

            return nearbyEvents;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения событий для карты: {ex.Message}");
            return new List<MapEvent>();
        }
    }
}