using System.Text;
using Newtonsoft.Json;
using Point_v1.Models;

namespace Point_v1.Services;

public class FirebaseRestService
{
    private readonly HttpClient _httpClient;
    private const string FirebaseUrl = "https://point-v1-default-rtdb.europe-west1.firebasedatabase.app/";
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

    public async Task<bool> DeleteAccountAsync(string idToken)
    {
        try
        {
            var deleteRequest = new
            {
                idToken = idToken
            };

            var json = JsonConvert.SerializeObject(deleteRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:delete?key={ApiKey}",
                content);

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Аккаунт успешно удален");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка удаления аккаунта: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка удаления аккаунта: {ex.Message}");
            return false;
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

    // НОВЫЕ МЕТОДЫ ДЛЯ ОБНОВЛЕНИЯ И УДАЛЕНИЯ СОБЫТИЙ
    public async Task<bool> UpdateEventAsync(Event eventItem)
    {
        try
        {
            if (string.IsNullOrEmpty(eventItem.Id))
            {
                System.Diagnostics.Debug.WriteLine($"❌ Не удалось обновить событие: отсутствует ID");
                return false;
            }

            var json = JsonConvert.SerializeObject(eventItem);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"{FirebaseUrl}events/{eventItem.Id}.json",
                content);

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Событие {eventItem.Id} успешно обновлено");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления события: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления события: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteEventAsync(string eventId)
    {
        try
        {
            if (string.IsNullOrEmpty(eventId))
            {
                System.Diagnostics.Debug.WriteLine($"❌ Не удалось удалить событие: отсутствует ID");
                return false;
            }

            var response = await _httpClient.DeleteAsync(
                $"{FirebaseUrl}events/{eventId}.json");

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Событие {eventId} успешно удалено");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка удаления события: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка удаления события: {ex.Message}");
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
    public async Task<List<Report>> GetReportsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{FirebaseUrl}/reports.json");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var reportsDict = JsonConvert.DeserializeObject<Dictionary<string, Report>>(json);
                return reportsDict?.Select(kvp =>
                {
                    kvp.Value.Id = kvp.Key;
                    return kvp.Value;
                }).ToList() ?? new List<Report>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки жалоб: {ex.Message}");
        }
        return new List<Report>();
    }

    public async Task<bool> AddReportAsync(Report report)
    {
        try
        {
            var json = JsonConvert.SerializeObject(report);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{FirebaseUrl}/reports.json", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка добавления жалобы: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateReportAsync(Report report)
    {
        try
        {
            var json = JsonConvert.SerializeObject(report);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{FirebaseUrl}/reports/{report.Id}.json", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления жалобы: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AddAuditLogAsync(AuditLog auditLog)
    {
        try
        {
            var json = JsonConvert.SerializeObject(auditLog);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{FirebaseUrl}/audit_logs.json", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка добавления лога: {ex.Message}");
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
