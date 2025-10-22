using Point_v1.Models;

namespace Point_v1.Services;

public interface IMapService
{
    Task<Location> GetCurrentLocationAsync();
    Task<List<MapEvent>> GetEventsNearbyAsync(Location center, double radiusKm = 5);
}