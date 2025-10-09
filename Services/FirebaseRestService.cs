using System.Text;
using Newtonsoft.Json;
using Point_v1.Models;

namespace Point_v1.Services;

public class FirebaseRestService
{
    private readonly HttpClient _httpClient;
    private const string FirebaseUrl = "https://point-v1-default-rtdb.firebaseio.com/";
    private const string ApiKey = "AIzaSyARDY_p3yQRGIeGcS7w5FSQs075Kh2OQiU";

    public FirebaseRestService()
    {
        _httpClient = new HttpClient();
    }

    // Аутентификация через REST API
    public async Task<string> SignInWithEmailAndPassword(string email, string password)
    {
        try
        {
            var authRequest = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var json = JsonConvert.SerializeObject(authRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={ApiKey}",
                content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResult = JsonConvert.DeserializeObject<FirebaseAuthResponse>(responseContent);
                return authResult?.IdToken;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка аутентификации: {ex.Message}");
            return null;
        }
    }

    public async Task<string> CreateUserWithEmailAndPassword(string email, string password, string displayName)
    {
        try
        {
            var authRequest = new
            {
                email = email,
                password = password,
                displayName = displayName,
                returnSecureToken = true
            };

            var json = JsonConvert.SerializeObject(authRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={ApiKey}",
                content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResult = JsonConvert.DeserializeObject<FirebaseAuthResponse>(responseContent);
                return authResult?.IdToken;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка регистрации: {ex.Message}");
            return null;
        }
    }

    // Работа с данными
    public async Task<List<Event>> GetEventsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{FirebaseUrl}events.json");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var eventsDict = JsonConvert.DeserializeObject<Dictionary<string, Event>>(content);

                if (eventsDict != null)
                {
                    return eventsDict.Select(kvp =>
                    {
                        kvp.Value.Id = kvp.Key;
                        return kvp.Value;
                    }).ToList();
                }
            }
            return new List<Event>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки событий: {ex.Message}");
            return new List<Event>();
        }
    }

    public async Task<bool> AddEventAsync(Event eventItem)
    {
        try
        {
            var json = JsonConvert.SerializeObject(eventItem);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{FirebaseUrl}events.json",
                content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка добавления события: {ex.Message}");
            return false;
        }
    }
}

public class FirebaseAuthResponse
{
    [JsonProperty("idToken")]
    public string IdToken { get; set; } = string.Empty;

    [JsonProperty("localId")]
    public string LocalId { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;
}