using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class EventDetailsViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IAuthStateService _authStateService;
    private readonly IReportService _reportService;
    private string _eventId;
    private readonly NavigationStateService _navigationService;


    public EventDetailsViewModel(IDataService dataService, IAuthStateService authStateService, IReportService reportService)
    {
        _dataService = dataService;
        _authStateService = authStateService;
        _reportService = reportService;

        ReportEventCommand = new Command(async () => await ReportEvent());


        System.Diagnostics.Debug.WriteLine($"✅ EventDetailsViewModel создан");
        System.Diagnostics.Debug.WriteLine($"✅ DataService: {_dataService != null}");
        System.Diagnostics.Debug.WriteLine($"✅ AuthStateService: {_authStateService != null}");
        System.Diagnostics.Debug.WriteLine($"✅ ReportService: {_reportService != null}");
        System.Diagnostics.Debug.WriteLine($"✅ EventId при создании: {_eventId}");



        ToggleParticipationCommand = new Command(async () => await ToggleParticipation());
        GoBackCommand = new Command(async () => await GoToHome());
        GoToLoginCommand = new Command(async () => await GoToLogin());
        EditEventCommand = new Command(() => StartEditing());
        SaveEditCommand = new Command(async () => await SaveEvent());
        CancelEditCommand = new Command(() => CancelEditing());
        DeleteEventCommand = new Command(async () => await DeleteEvent());
    }

    public bool ShowOrganizerButtons
    {
        get
        {
            var show = IsAuthenticated && IsCreator && !IsEditing;
            System.Diagnostics.Debug.WriteLine($"🎯 ShowOrganizerButtons: {show} " +
                                             $"(Auth: {IsAuthenticated}, Creator: {IsCreator}, Editing: {IsEditing})");
            return show;
        }
    }


    public string EventId
    {
        get => _eventId;
        set
        {
            if (SetProperty(ref _eventId, value))
            {
                System.Diagnostics.Debug.WriteLine($"🎯 EventId установлен в ViewModel: {value}");

                if (!string.IsNullOrEmpty(value))
                {
                    _ = LoadEventDetails();
                }
            }
        }
    }
    public ICommand ReportEventCommand { get; }


    private bool _cameFromReports;
    public bool CameFromReports
    {
        get => _cameFromReports;
        set => SetProperty(ref _cameFromReports, value);
    }
    private async Task ReportEvent()
    {
        if (!_authStateService.IsAuthenticated)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Войдите, чтобы отправить жалобу", "OK");
            return;
        }

        try
        {
            if (string.IsNullOrEmpty(_eventId))
            {
                System.Diagnostics.Debug.WriteLine("❌ EventId не установлен в ViewModel!");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось определить событие", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"🎯 Начало создания жалобы на событие: {_eventId}");

            var eventItem = await _dataService.GetEventAsync(_eventId);
            if (eventItem == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Событие {_eventId} не найдено!");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Событие не найдено", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✅ Событие найдено: {eventItem.Title} (ID: {eventItem.Id})");

            var reason = await Application.Current.MainPage.DisplayActionSheet(
                $"Жалоба на событие: {eventItem.Title}",
                "Отмена",
                null,
                "📧 Спам",
                "🚫 Неуместный контент",
                "💸 Мошенничество",
                "⚖️ Нелегальное",
                "❓ Другое"
            );

            if (reason != "Отмена")
            {
                var reportType = reason switch
                {
                    "📧 Спам" => ReportType.Spam,
                    "🚫 Неуместный контент" => ReportType.Inappropriate,
                    "💸 Мошенничество" => ReportType.Scam,
                    "⚖️ Нелегальное" => ReportType.Illegal,
                    "❓ Другое" => ReportType.Other,
                    _ => ReportType.Other
                };

                var customReason = reason;
                if (reason == "❓ Другое")
                {
                    customReason = await Application.Current.MainPage.DisplayPromptAsync(
                        "Уточните причину",
                        "Опишите проблему:",
                        "Отправить",
                        "Отмена",
                        maxLength: 500
                    );

                    if (string.IsNullOrEmpty(customReason))
                        return;
                }

                System.Diagnostics.Debug.WriteLine($"📤 Отправка жалобы: EventId={_eventId}, Type={reportType}, Reason={customReason}");

                var success = await _reportService.CreateReportAsync(
                    _eventId, // ИСПОЛЬЗУЕМ _eventId ИЗ ViewModel
                    _authStateService.CurrentUserId,
                    reportType,
                    customReason
                );

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Жалоба успешно создана");
                    await Application.Current.MainPage.DisplayAlert(
                        "Спасибо!",
                        $"Жалоба на событие \"{eventItem.Title}\" отправлена модераторам",
                        "OK"
                    );
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Не удалось создать жалобу");
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        "Не удалось отправить жалобу",
                        "OK"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка отправки жалобы: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось отправить жалобу", "OK");
        }
    }

    private bool _isEditing = false;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }



    private Event _event;
    public Event Event
    {
        get => _event;
        set
        {
            SetProperty(ref _event, value);
            System.Diagnostics.Debug.WriteLine($"📦 Event установлен: {value?.Title ?? "null"}");

            if (value != null)
            {
                UpdateParticipationState();
                _ = LoadOrganizerAvatar(); // Загружаем аватар организатора
            }

            OnPropertyChanged(nameof(CanParticipate));
        }
    }

    private ImageSource _organizerAvatar;
    public ImageSource OrganizerAvatar
    {
        get => _organizerAvatar;
        set
        {
            SetProperty(ref _organizerAvatar, value);
            OnPropertyChanged(nameof(HasOrganizerAvatar));
        }
    }

    public bool HasOrganizerAvatar => _organizerAvatar != null;


    private bool _isParticipating;
    public bool IsParticipating
    {
        get => _isParticipating;
        set
        {
            SetProperty(ref _isParticipating, value);
            UpdateParticipationButton();
            OnPropertyChanged(nameof(CanParticipate));
        }
    }


    private string _participationButtonText = "Я пойду!";
    public string ParticipationButtonText
    {
        get => _participationButtonText;
        set => SetProperty(ref _participationButtonText, value);
    }

    private Color _participationButtonColor = Color.FromArgb("#512BD4");
    public Color ParticipationButtonColor
    {
        get => _participationButtonColor;
        set => SetProperty(ref _participationButtonColor, value);
    }

    private string _editTitle;
    public string EditTitle
    {
        get => _editTitle;
        set => SetProperty(ref _editTitle, value);
    }

    private string _editDescription;
    public string EditDescription
    {
        get => _editDescription;
        set => SetProperty(ref _editDescription, value);
    }

    private int _editMaxParticipants;
    public int EditMaxParticipants
    {
        get => _editMaxParticipants;
        set => SetProperty(ref _editMaxParticipants, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsAuthenticated => _authStateService?.IsAuthenticated ?? false;
    public bool IsGuestMode => !IsAuthenticated;

    public ICommand ToggleParticipationCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand GoToLoginCommand { get; }
    public ICommand OpenChatCommand { get; }
    public ICommand EditEventCommand { get; }
    public ICommand SaveEditCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteEventCommand { get; }



    private bool _isCreator;
    public bool IsCreator
    {
        get => _isCreator;
        set
        {
            SetProperty(ref _isCreator, value);
            OnPropertyChanged(nameof(CanParticipate));
        }
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🎯 Query параметры: {string.Join(", ", query.Keys)}");

            if (query.ContainsKey("eventId"))
            {
                var eventId = query["eventId"] as string;
                if (!string.IsNullOrEmpty(eventId))
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 Получен eventId из query: {eventId}");
                    EventId = eventId;
                }
            }

            CameFromReports = query.ContainsKey("fromReports") && query["fromReports"]?.ToString() == "true";

            if (CameFromReports)
            {
                System.Diagnostics.Debug.WriteLine("📍 Пришли со страницы жалоб");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("📍 Пришли из обычной навигации");
            }

            if (!string.IsNullOrEmpty(EventId))
            {
                _ = LoadEventDetails();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в ApplyQueryAttributes: {ex.Message}");
        }
    }
    public bool CanParticipate
    {
        get
        {
            var canParticipate = IsAuthenticated &&
                               !IsCreator &&
                               Event != null &&
                               !Event.IsFull &&
                               Event.EventDate > DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"🎯 CanParticipate вычислено: {canParticipate} " +
                                             $"(Auth: {IsAuthenticated}, Creator: {IsCreator}, " +
                                             $"Event: {Event != null}, Full: {Event?.IsFull}, " +
                                             $"Future: {Event?.EventDate > DateTime.Now})");

            return canParticipate;
        }
    }

    private async Task GoToHome()
    {
        System.Diagnostics.Debug.WriteLine("🔙 Выполняется команда Назад");

        try
        {
            if (CameFromReports)
            {
                System.Diagnostics.Debug.WriteLine("🔙 Возврат к жалобам");
                await Shell.Current.GoToAsync("ReportsManagementPage");
            }
            else
            {
                bool success = false;
                try
                {
                    await Shell.Current.GoToAsync("..");
                    System.Diagnostics.Debug.WriteLine("✅ Успешный возврат назад по стеку");
                    success = true;
                }
                catch (Exception ex1)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Возврат по стеку не сработал: {ex1.Message}");
                }
                if (!success)
                {
                    try
                    {
                        await Shell.Current.GoToAsync("///HomePage");
                        System.Diagnostics.Debug.WriteLine("✅ Успешный переход на главную");
                        success = true;
                    }
                    catch (Exception ex2)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Переход на главную не сработал: {ex2.Message}");
                    }
                }

                if (!success)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось вернуться", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Общая ошибка возврата: {ex.Message}");
            await Shell.Current.GoToAsync("///HomePage");
        }
    }

    private async Task GoToLogin()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private void StartEditing()
    {
        if (Event == null) return;

        System.Diagnostics.Debug.WriteLine($"✏️ Начало редактирования события: {Event.Title}");

   
        EditTitle = Event.Title;
        EditDescription = Event.Description;
        EditMaxParticipants = Event.MaxParticipants;

        IsEditing = true;
        OnPropertyChanged(nameof(ShowOrganizerButtons));
        System.Diagnostics.Debug.WriteLine($"✏️ Режим редактирования: {IsEditing}");
    }

    private void CancelEditing()
    {
        System.Diagnostics.Debug.WriteLine($"❌ Отмена редактирования");
        IsEditing = false;
        EditTitle = "";
        EditDescription = "";
        EditMaxParticipants = 20;
        OnPropertyChanged(nameof(ShowOrganizerButtons));
        System.Diagnostics.Debug.WriteLine($"✏️ Режим редактирования: {IsEditing}");
    }

    private async Task SaveEvent()
    {
        if (Event == null || IsLoading) return;

        try
        {
            IsLoading = true;

            Event.Title = EditTitle?.Trim() ?? Event.Title;
            Event.Description = EditDescription?.Trim() ?? Event.Description;
            Event.MaxParticipants = EditMaxParticipants;

            var success = await _dataService.UpdateEventAsync(Event);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Успех!", "Событие обновлено", "OK");
                IsEditing = false;
                OnPropertyChanged(nameof(ShowOrganizerButtons));

                OnPropertyChanged(nameof(Event));
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось обновить событие", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка сохранения события: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось обновить событие", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }


    private async Task DeleteEvent()
    {
        if (Event == null) return;

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Подтверждение удаления",
            $"Вы уверены, что хотите удалить событие \"{Event.Title}\"? Это действие нельзя отменить.",
            "Удалить",
            "Отмена"
        );

        if (!confirm) return;

        try
        {
            IsLoading = true;

            var success = await _dataService.DeleteEventAsync(Event.Id);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Успех!", "Событие удалено", "OK");

                await UpdateUserStatistics();
                await GoToHome();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось удалить событие", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка удаления события: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось удалить событие", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
    public async Task LoadEventDetails()
    {
        if (string.IsNullOrEmpty(_eventId))
        {
            System.Diagnostics.Debug.WriteLine("❌ EventId пустой - не могу загрузить событие");
            return;
        }

        if (IsLoading) return;

        try
        {
            IsLoading = true;
            System.Diagnostics.Debug.WriteLine($"🔄 Загрузка события: {_eventId}");

            var eventItem = await _dataService.GetEventAsync(_eventId);

            if (eventItem == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Событие {_eventId} не найдено в DataService");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✅ Событие загружено: {eventItem.Title}");
            System.Diagnostics.Debug.WriteLine($"📋 CategoryIds: {string.Join(", ", eventItem.CategoryIds ?? new List<string>())}");
            System.Diagnostics.Debug.WriteLine($"📋 DisplayCategories: {string.Join(", ", eventItem.DisplayCategories)}");

            Event = eventItem;
            UpdateParticipationState();

            OnPropertyChanged(nameof(Event));
            OnPropertyChanged(nameof(CanParticipate));
            OnPropertyChanged(nameof(ShowOrganizerButtons));
            
            if (Event != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnPropertyChanged(nameof(Event));
                });
            }

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки события: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateParticipationState()
    {
        if (Event == null || !IsAuthenticated || string.IsNullOrEmpty(_authStateService.CurrentUserId))
        {
            IsParticipating = false;
            IsCreator = false;
            return;
        }

        try
        {
            IsParticipating = Event.ParticipantIds?.Contains(_authStateService.CurrentUserId) == true;
            IsCreator = Event.CreatorId == _authStateService.CurrentUserId;

            System.Diagnostics.Debug.WriteLine($"🎯 Статус участия: {IsParticipating}, Организатор: {IsCreator}");
            System.Diagnostics.Debug.WriteLine($"🎯 CurrentUserId: {_authStateService.CurrentUserId}");
            System.Diagnostics.Debug.WriteLine($"🎯 Event.CreatorId: {Event.CreatorId}");

            OnPropertyChanged(nameof(CanParticipate));
            OnPropertyChanged(nameof(ShowOrganizerButtons));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка в UpdateParticipationState: {ex.Message}");
            IsParticipating = false;
            IsCreator = false;
        }
    }



    private void UpdateParticipationButton()
    {
        if (IsParticipating)
        {
            ParticipationButtonText = "Я не пойду";
            ParticipationButtonColor = Color.FromArgb("#FF6B6B");
        }
        else
        {
            ParticipationButtonText = "Я пойду!";
            ParticipationButtonColor = Color.FromArgb("#512BD4");
        }
        System.Diagnostics.Debug.WriteLine($"🔄 Текст кнопки обновлен: {ParticipationButtonText}");
    }


    private async Task ToggleParticipation()
    {
        System.Diagnostics.Debug.WriteLine("🎯 Нажата кнопка участия");

        if (!IsAuthenticated)
        {
            await Application.Current.MainPage.DisplayAlert("Требуется вход", "Войдите, чтобы участвовать в событиях", "OK");
            return;
        }

        if (Event?.EventDate <= DateTime.Now)
        {
            await Application.Current.MainPage.DisplayAlert("Событие завершено", "Нельзя участвовать в прошедших событиях", "OK");
            return;
        }

        if (Event == null)
        {
            System.Diagnostics.Debug.WriteLine("❌ Event is null - перезагружаем...");
            await LoadEventDetails();
            return;
        }

        try
        {
            bool success;

            if (IsParticipating)
            {
                System.Diagnostics.Debug.WriteLine("➖ Выход из события");
                success = await _dataService.LeaveEventAsync(_eventId, _authStateService.CurrentUserId);
                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Успех", "Вы больше не участвуете в событии", "OK");
                }
            }
            else
            {
                if (Event.IsFull)
                {
                    await Application.Current.MainPage.DisplayAlert("Мест нет", "На это событие уже нет свободных мест", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("➕ Присоединение к событию");
                success = await _dataService.JoinEventAsync(_eventId, _authStateService.CurrentUserId);
                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Успех", "Вы присоединились к событию!", "OK");
                }
            }

            if (success)
            {
                await UpdateUserStatistics();
                await LoadEventDetails();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось изменить участие", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка участия: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось изменить участие", "OK");
        }
    }


    private async Task UpdateUserStatistics()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("📊 Статистика будет обновлена при следующем открытии профиля");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления статистики: {ex.Message}");
        }
    }

    private async Task LoadOrganizerAvatar()
    {
        try
        {
            if (Event == null || string.IsNullOrEmpty(Event.CreatorId))
            {
                OrganizerAvatar = null;
                OnPropertyChanged(nameof(HasOrganizerAvatar));
                return;
            }

            var organizer = await _dataService.GetUserAsync(Event.CreatorId);
            if (organizer != null && !string.IsNullOrEmpty(organizer.AvatarUrl))
            {
                if (organizer.AvatarUrl.StartsWith("local:"))
                {
                    var localPath = organizer.AvatarUrl.Substring(6); // Убираем префикс "local:"
                    if (File.Exists(localPath))
                    {
                        OrganizerAvatar = ImageSource.FromFile(localPath);
                        System.Diagnostics.Debug.WriteLine($"📸 Аватар организатора загружен из локального хранилища: {localPath}");
                        OnPropertyChanged(nameof(HasOrganizerAvatar));
                        return;
                    }
                }
                else
                {
                    try
                    {
                        OrganizerAvatar = ImageSource.FromUri(new Uri(organizer.AvatarUrl));
                        System.Diagnostics.Debug.WriteLine($"📸 Аватар организатора загружен из URL: {organizer.AvatarUrl}");
                        OnPropertyChanged(nameof(HasOrganizerAvatar));
                        return;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Ошибка загрузки аватара организатора из URL: {ex.Message}");
                        
                        var localPath = Path.Combine(FileSystem.AppDataDirectory, "Avatars", $"avatar_{Event.CreatorId}.jpg");
                        if (File.Exists(localPath))
                        {
                            OrganizerAvatar = ImageSource.FromFile(localPath);
                            System.Diagnostics.Debug.WriteLine($"📸 Аватар организатора загружен из локального хранилища (резервный): {localPath}");
                            OnPropertyChanged(nameof(HasOrganizerAvatar));
                            return;
                        }
                    }
                }
            }
            
            var fallbackLocalPath = Path.Combine(FileSystem.AppDataDirectory, "Avatars", $"avatar_{Event.CreatorId}.jpg");
            if (File.Exists(fallbackLocalPath))
            {
                OrganizerAvatar = ImageSource.FromFile(fallbackLocalPath);
                System.Diagnostics.Debug.WriteLine($"📸 Аватар организатора загружен из локального хранилища (прямая проверка): {fallbackLocalPath}");
                OnPropertyChanged(nameof(HasOrganizerAvatar));
                return;
            }
            
            OrganizerAvatar = null;
            OnPropertyChanged(nameof(HasOrganizerAvatar));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки аватара организатора: {ex.Message}");
            OrganizerAvatar = null;
            OnPropertyChanged(nameof(HasOrganizerAvatar));
        }
    }
    private async Task CheckDataServiceAndLoadEvent(string eventId)
    {
        try
        {
            await LoadEventDetails();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки события: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось загрузить событие", "OK");
        }
    }



}