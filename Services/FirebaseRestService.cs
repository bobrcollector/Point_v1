using System.Text;
using Newtonsoft.Json;
using Point_v1.Models;

namespace Point_v1.Services;

public class FirebaseRestService
{
    private readonly HttpClient _httpClient;
    private const string FirebaseUrl = "https://point-v1-default-rtdb.europe-west1.firebasedatabase.app/"; // ОБНОВИ ЭТУ СТРОКУ
    private const string ApiKey = "AIzaSyAEzmKGE5xr4u2ggze_eTuYyKfVr823vJs";

    public FirebaseRestService()
    {
        _httpClient = new HttpClient();
    }

    // Аутентификация через REST API
    public async Task<FirebaseAuthResponse> SignInWithEmailAndPassword(string email, string password)
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
                return authResult;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка Firebase: {errorContent}");
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка аутентификации: {ex.Message}");
            return null;
        }
    }

    public async Task<FirebaseAuthResponse> CreateUserWithEmailAndPassword(string email, string password, string displayName)
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
                return authResult;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка Firebase: {errorContent}");
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка регистрации: {ex.Message}");
            return null;
        }
    }

    // Работа с событиями
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

    // Работа с пользователями
    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{FirebaseUrl}users.json");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var usersDict = JsonConvert.DeserializeObject<Dictionary<string, User>>(content);

                if (usersDict != null)
                {
                    return usersDict.Select(kvp =>
                    {
                        kvp.Value.Id = kvp.Key;
                        return kvp.Value;
                    }).ToList();
                }
            }
            return new List<User>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки пользователей: {ex.Message}");
            return new List<User>();
        }
    }

    public async Task<bool> AddOrUpdateUserAsync(User user)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔗 Отправка запроса к: {FirebaseUrl}users/{user.Id}.json");

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"{FirebaseUrl}users/{user.Id}.json",
                content);

            System.Diagnostics.Debug.WriteLine($"📡 Ответ сервера: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка Firebase: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка сохранения пользователя: {ex.Message}");
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