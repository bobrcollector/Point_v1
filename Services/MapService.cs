using Point_v1.Models;

namespace Point_v1.Services;

public class MapService : IMapService
{
    private readonly IDataService _dataService;

    // Поля для rate limiting
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly object _lockObject = new object();

    public MapService(IDataService dataService)
    {
        _dataService = dataService;
    }

    // Метод для соблюдения rate limit (1 запрос в секунду)
    private async Task WaitForRateLimit()
    {
        lock (_lockObject)
        {
            var timeSinceLastRequest = DateTime.Now - _lastRequestTime;
            if (timeSinceLastRequest.TotalSeconds < 1.0)
            {
                var delay = TimeSpan.FromSeconds(1.0) - timeSinceLastRequest;
                System.Threading.Thread.Sleep(delay);
            }
            _lastRequestTime = DateTime.Now;
        }
        await Task.Delay(100); // Небольшая дополнительная задержка
    }

    public async Task<Location> GetCurrentLocationAsync()
    {
        // Этот метод НЕ требует WaitForRateLimit, так как использует локальные сервисы
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

        return new Location(55.7558, 37.6173);
    }

    public async Task<List<MapEvent>> GetEventsNearbyAsync(Location center, double radiusKm = 5)
    {
        // Этот метод НЕ требует WaitForRateLimit, так как работает с локальной БД
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

    public async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
    {
        await WaitForRateLimit();

        try
        {
            System.Diagnostics.Debug.WriteLine($"📍 Яндекс обратное геокодирование: {latitude}, {longitude}");

            // Яндекс Обратное геокодирование
            var url = $"https://geocode-maps.yandex.ru/1.x/?apikey=1a0b162d-9aa4-4d51-8441-151469a3c82a&format=json&geocode={longitude},{latitude}&lang=ru_RU";

            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);

            var json = System.Text.Json.JsonDocument.Parse(response);
            var members = json.RootElement
                .GetProperty("response")
                .GetProperty("GeoObjectCollection")
                .GetProperty("featureMember");

            if (members.GetArrayLength() > 0)
            {
                var address = members.EnumerateArray().First()
                    .GetProperty("GeoObject")
                    .GetProperty("metaDataProperty")
                    .GetProperty("GeocoderMetaData")
                    .GetProperty("text")
                    .GetString();

                System.Diagnostics.Debug.WriteLine($"📍 Яндекс обратное геокодирование успешно: {address}");
                return address;
            }

            return $"Широта: {latitude:F4}, Долгота: {longitude:F4}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка Яндекс обратного геокодирования: {ex.Message}");
            return $"Широта: {latitude:F4}, Долгота: {longitude:F4}";
        }
    }

    public async Task<Location> GetCoordinatesFromAddressAsync(string address)
    {
        await WaitForRateLimit();

        try
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            System.Diagnostics.Debug.WriteLine($"🔍 Поиск координат через Яндекс для: {address}");

            // Яндекс Геокодер API
            var url = $"https://geocode-maps.yandex.ru/1.x/?apikey=1a0b162d-9aa4-4d51-8441-151469a3c82a&format=json&geocode={Uri.EscapeDataString(address)}&lang=ru_RU";

            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);

            var json = System.Text.Json.JsonDocument.Parse(response);
            var members = json.RootElement
                .GetProperty("response")
                .GetProperty("GeoObjectCollection")
                .GetProperty("featureMember");

            if (members.GetArrayLength() > 0)
            {
                var pos = members.EnumerateArray().First()
                    .GetProperty("GeoObject")
                    .GetProperty("Point")
                    .GetProperty("pos")
                    .GetString();

                var coords = pos.Split(' ');
                if (coords.Length == 2 &&
                    double.TryParse(coords[1], out double lat) &&
                    double.TryParse(coords[0], out double lon))
                {
                    System.Diagnostics.Debug.WriteLine($"📍 Найдены координаты через Яндекс: {lat}, {lon}");
                    return new Location(lat, lon);
                }
            }

            return new Location(55.7558, 37.6173);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка Яндекс геокодирования: {ex.Message}");
            return new Location(55.7558, 37.6173);
        }
    }

    public async Task<List<string>> GetAddressSuggestionsAsync(string query)
    {
        await WaitForRateLimit();

        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<string>();

            System.Diagnostics.Debug.WriteLine($"🔍 Поиск подсказок через Яндекс для: {query}");

            // Яндекс Геокодер API для автодополнения
            var url = $"https://geocode-maps.yandex.ru/1.x/?apikey=1a0b162d-9aa4-4d51-8441-151469a3c82a&format=json&geocode={Uri.EscapeDataString(query)}&lang=ru_RU&results=5";

            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);

            var json = System.Text.Json.JsonDocument.Parse(response);
            var members = json.RootElement
                .GetProperty("response")
                .GetProperty("GeoObjectCollection")
                .GetProperty("featureMember");

            var suggestions = new List<string>();

            foreach (var member in members.EnumerateArray())
            {
                if (member.TryGetProperty("GeoObject", out var geoObject))
                {
                    if (geoObject.TryGetProperty("metaDataProperty", out var metaData) &&
                        metaData.TryGetProperty("GeocoderMetaData", out var geocoderMetaData) &&
                        geocoderMetaData.TryGetProperty("text", out var text))
                    {
                        var address = text.GetString();
                        if (!string.IsNullOrEmpty(address))
                        {
                            suggestions.Add(address);
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"✅ Найдено подсказок через Яндекс: {suggestions.Count}");
            return suggestions;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка Яндекс поиска: {ex.Message}");
            return GetFallbackSuggestions(query);
        }
    }

    // Вспомогательные методы (без изменений)
    private string SimplifyAddress(string fullAddress)
    {
        if (string.IsNullOrEmpty(fullAddress))
            return fullAddress;

        var parts = fullAddress.Split(',');
        if (parts.Length > 3)
        {
            return string.Join(", ", parts.Take(4)).Trim();
        }

        return fullAddress;
    }

    private List<string> GetFallbackSuggestions(string query)
    {
        var fallbackSuggestions = new List<string>
        {
            "Москва, Красная площадь",
            "Москва, Тверская улица",
            "Москва, Старый Арбат",
            "Москва, ВДНХ",
            "Москва, Московский Кремль"
        };

        return fallbackSuggestions
            .Where(s => s.ToLower().Contains(query.ToLower()))
            .Take(5)
            .ToList();
    }
}