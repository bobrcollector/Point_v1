using Point_v1.Models;
using Point_v1.Services;
using System.Windows.Input;

namespace Point_v1.ViewModels;

public class EventDetailsViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly IAuthStateService _authStateService;
    private string _eventId;

    public EventDetailsViewModel(IDataService dataService, IAuthStateService authStateService)
    {
        _dataService = dataService;
        _authStateService = authStateService;

        // Проверяем инициализацию сервисов
        System.Diagnostics.Debug.WriteLine($"✅ EventDetailsViewModel создан");
        System.Diagnostics.Debug.WriteLine($"✅ DataService: {_dataService != null}");
        System.Diagnostics.Debug.WriteLine($"✅ AuthStateService: {_authStateService != null}");
        System.Diagnostics.Debug.WriteLine($"✅ IsAuthenticated: {IsAuthenticated}");

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

    private bool _isEditing = false;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }
    public string EventId
    {
        get => _eventId;
        set
        {
            if (SetProperty(ref _eventId, value) && !string.IsNullOrEmpty(value))
            {
                System.Diagnostics.Debug.WriteLine($"🎯 EventId установлен: {value}");
                // ДОБАВИМ ПРОВЕРКУ DataService
                _ = CheckDataServiceAndLoadEvent(value);
            }
        }
    }

    private Event _event;
    public Event Event
    {
        get => _event;
        set
        {
            SetProperty(ref _event, value);
            System.Diagnostics.Debug.WriteLine($"📦 Event установлен: {value?.Title ?? "null"}");

            // Обновляем состояние при изменении события
            if (value != null)
            {
                UpdateParticipationState();
            }

            // ДОБАВЛЯЕМ ОБНОВЛЕНИЕ CanParticipate
            OnPropertyChanged(nameof(CanParticipate));
        }
    }


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
        System.Diagnostics.Debug.WriteLine("🔙 Выполняется команда Назад (на главную)");

        try
        {
            await Shell.Current.GoToAsync("//HomePage");
            System.Diagnostics.Debug.WriteLine("✅ Успешный переход на главную страницу");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка перехода на главную: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось вернуться на главную", "OK");
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
        // Очищаем поля редактирования
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

            // Обновляем событие из полей редактирования
            Event.Title = EditTitle?.Trim() ?? Event.Title;
            Event.Description = EditDescription?.Trim() ?? Event.Description;
            Event.MaxParticipants = EditMaxParticipants;

            var success = await _dataService.UpdateEventAsync(Event);

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Успех!", "Событие обновлено", "OK");
                IsEditing = false;
                OnPropertyChanged(nameof(ShowOrganizerButtons));

                // Обновляем отображение
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
        if (string.IsNullOrEmpty(_eventId) || IsLoading) return;

        try
        {
            IsLoading = true;
            System.Diagnostics.Debug.WriteLine($"🔄 Загрузка события: {_eventId}");

            var eventItem = await _dataService.GetEventAsync(_eventId);

            if (eventItem == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Событие {_eventId} не найдено в DataService");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Событие не найдено", "OK");
                await GoToHome(); // Возвращаем на главную если событие не найдено
                return;
            }

            // ОТЛАДОЧНАЯ ИНФОРМАЦИЯ
            System.Diagnostics.Debug.WriteLine($"✅ Событие загружено: {eventItem.Title}");
            System.Diagnostics.Debug.WriteLine($"👤 CreatorId: {eventItem.CreatorId}");
            System.Diagnostics.Debug.WriteLine($"👤 CreatorName: {eventItem.CreatorName}");
            System.Diagnostics.Debug.WriteLine($"📅 EventDate: {eventItem.EventDate}");
            System.Diagnostics.Debug.WriteLine($"📍 Address: {eventItem.Address ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"📝 Description: {eventItem.Description ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"👥 Participants: {eventItem.ParticipantIds?.Count ?? 0}");

            Event = eventItem;
            UpdateParticipationState();

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки события: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось загрузить событие", "OK");
            await GoToHome();
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

            // ДОБАВЛЯЕМ ОБНОВЛЕНИЕ CanParticipate И ShowOrganizerButtons
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

        // ДОБАВЛЯЕМ ПРОВЕРКУ НА ПРОШЕДШИЕ СОБЫТИЯ
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
                // Перезагружаем данные события
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
            // Просто логируем - статистика обновится при следующем открытии профиля
            System.Diagnostics.Debug.WriteLine("📊 Статистика будет обновлена при следующем открытии профиля");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления статистики: {ex.Message}");
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