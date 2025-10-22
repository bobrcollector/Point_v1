using System.Text.Json;

namespace Point_v1.Services;

public class YandexGeocodingService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public YandexGeocodingService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string address)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            System.Diagnostics.Debug.WriteLine($"🗺️ Геокодирование адреса: {address}");

            var url = $"https://geocode-maps.yandex.ru/1.x/?format=json&apikey={_apiKey}&geocode={Uri.EscapeDataString(address)}";

            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            // Парсим ответ Яндекс Геокодера
            var featureMember = json.RootElement
                .GetProperty("response")
                .GetProperty("GeoObjectCollection")
                .GetProperty("featureMember");

            if (featureMember.GetArrayLength() == 0)
            {
                System.Diagnostics.Debug.WriteLine("❌ Адрес не найден");
                return null;
            }

            var pos = featureMember[0]
                .GetProperty("GeoObject")
                .GetProperty("Point")
                .GetProperty("pos")
                .GetString();

            if (string.IsNullOrEmpty(pos))
            {
                System.Diagnostics.Debug.WriteLine("❌ Координаты не найдены");
                return null;
            }

            var coords = pos.Split(' ');
            var longitude = double.Parse(coords[0]);
            var latitude = double.Parse(coords[1]);

            System.Diagnostics.Debug.WriteLine($"✅ Координаты получены: {latitude}, {longitude}");

            return (latitude, longitude);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка геокодирования: {ex.Message}");
            return null;
        }
    }

    public async Task<string> GetAddressAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"https://geocode-maps.yandex.ru/1.x/?format=json&apikey={_apiKey}&geocode={longitude},{latitude}";

            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            var address = json.RootElement
                .GetProperty("response")
                .GetProperty("GeoObjectCollection")
                .GetProperty("featureMember")[0]
                .GetProperty("GeoObject")
                .GetProperty("metaDataProperty")
                .GetProperty("GeocoderMetaData")
                .GetProperty("text")
                .GetString();

            return address ?? "Адрес не найден";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обратного геокодирования: {ex.Message}");
            return "Ошибка получения адреса";
        }
    }
}