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

    // Загрузка аватара в Firebase Storage
    public async Task<string> UploadAvatarAsync(string filePath, string userId, string idToken)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"❌ Файл не найден: {filePath}");
                return null;
            }

            // Используем уникальный путь для каждого пользователя: avatars/{userId}.jpg
            // Это гарантирует, что каждый пользователь имеет свой уникальный аватар
            var fileName = $"avatars/{userId}.jpg";
            var bucketName = "point-v1.firebasestorage.app";
            
            // Используем правильный endpoint для загрузки через REST API v1
            var storageUrl = $"https://firebasestorage.googleapis.com/upload/storage/v1/b/{bucketName}/o?uploadType=media&name={Uri.EscapeDataString(fileName)}";
            
            System.Diagnostics.Debug.WriteLine($"📤 Загрузка аватара для пользователя {userId} в путь: {fileName}");
            System.Diagnostics.Debug.WriteLine($"📤 URL: {storageUrl}");

            using (var fileStream = File.OpenRead(filePath))
            {
                var content = new StreamContent(fileStream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Headers.ContentLength = fileStream.Length;

                var request = new HttpRequestMessage(HttpMethod.Post, storageUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", idToken);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                
                System.Diagnostics.Debug.WriteLine($"📡 Статус ответа: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var uploadResult = JsonConvert.DeserializeObject<FirebaseStorageUploadResponse>(responseContent);
                    
                    if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.Name))
                    {
                        // Получаем токен для скачивания (может быть строкой или массивом)
                        string downloadToken = null;
                        if (uploadResult.DownloadTokens != null)
                        {
                            if (uploadResult.DownloadTokens is string tokenStr)
                            {
                                downloadToken = tokenStr;
                            }
                            else if (uploadResult.DownloadTokens is Newtonsoft.Json.Linq.JArray tokenArray && tokenArray.Count > 0)
                            {
                                downloadToken = tokenArray[0].ToString();
                            }
                        }

                        if (!string.IsNullOrEmpty(downloadToken))
                        {
                            // Формируем публичный URL для скачивания
                            var downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/point-v1.firebasestorage.app/o/{Uri.EscapeDataString(fileName)}?alt=media&token={downloadToken}";
                            System.Diagnostics.Debug.WriteLine($"✅ Аватар загружен: {downloadUrl}");
                            return downloadUrl;
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки аватара (Status: {response.StatusCode}): {errorContent}");
                    
                    // Попробуем альтернативный метод загрузки через multipart
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        System.Diagnostics.Debug.WriteLine("🔄 Пробуем альтернативный метод загрузки...");
                        return await UploadAvatarMultipartAsync(filePath, userId, idToken, fileName);
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки аватара: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            return null;
        }
    }

    // Альтернативный метод загрузки через multipart/form-data
    private async Task<string> UploadAvatarMultipartAsync(string filePath, string userId, string idToken, string fileName)
    {
        try
        {
            var bucketName = "point-v1.firebasestorage.app";
            var storageUrl = $"https://firebasestorage.googleapis.com/upload/storage/v1/b/{bucketName}/o?uploadType=multipart&name={Uri.EscapeDataString(fileName)}";
            
            System.Diagnostics.Debug.WriteLine($"📤 Альтернативная загрузка (multipart): {storageUrl}");

            using (var fileStream = File.OpenRead(filePath))
            {
                var boundary = $"----WebKitFormBoundary{DateTime.UtcNow.Ticks}";
                
                // Создаем multipart content вручную для правильного формата
                var multipartContent = new MultipartContent("related", boundary);
                multipartContent.Headers.Remove("Content-Type");
                multipartContent.Headers.TryAddWithoutValidation("Content-Type", $"multipart/related; boundary={boundary}");
                
                // Первая часть - метаданные в JSON
                var metadata = new
                {
                    contentType = "image/jpeg"
                };
                var metadataJson = JsonConvert.SerializeObject(metadata);
                var metadataContent = new StringContent(metadataJson, Encoding.UTF8, "application/json");
                metadataContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                multipartContent.Add(metadataContent);
                
                // Вторая часть - файл
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                multipartContent.Add(fileContent);

                var request = new HttpRequestMessage(HttpMethod.Post, storageUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", idToken);
                request.Content = multipartContent;

                var response = await _httpClient.SendAsync(request);
                
                System.Diagnostics.Debug.WriteLine($"📡 Статус ответа (multipart): {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"✅ Ответ multipart: {responseContent}");
                    
                    var uploadResult = JsonConvert.DeserializeObject<FirebaseStorageUploadResponse>(responseContent);
                    
                    if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.Name))
                    {
                        // Получаем токен для скачивания
                        string downloadToken = null;
                        if (uploadResult.DownloadTokens != null)
                        {
                            if (uploadResult.DownloadTokens is string tokenStr)
                            {
                                downloadToken = tokenStr;
                            }
                            else if (uploadResult.DownloadTokens is Newtonsoft.Json.Linq.JArray tokenArray && tokenArray.Count > 0)
                            {
                                downloadToken = tokenArray[0].ToString();
                            }
                        }

                        if (!string.IsNullOrEmpty(downloadToken))
                        {
                            var downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{Uri.EscapeDataString(fileName)}?alt=media&token={downloadToken}";
                            System.Diagnostics.Debug.WriteLine($"✅ Аватар загружен (multipart): {downloadUrl}");
                            return downloadUrl;
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка multipart загрузки (Status: {response.StatusCode}): {errorContent}");
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка multipart загрузки: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            return null;
        }
    }
}

public class FirebaseStorageUploadResponse
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("bucket")]
    public string Bucket { get; set; }

    [JsonProperty("downloadTokens")]
    public object DownloadTokens { get; set; }
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
