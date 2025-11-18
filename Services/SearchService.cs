using Point_v1.Models;

namespace Point_v1.Services;

public class SearchService : ISearchService
{
    private readonly IDataService _dataService;

    public SearchService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<List<Event>> SearchEventsAsync(string query, string category, DateTime? date)
    {
        var allEvents = await _dataService.GetEventsAsync();

        if (allEvents == null) return new List<Event>();

        // ИСПРАВЛЕНИЕ: Исключаем завершенные, заблокированные и неактивные события
        var filteredEvents = allEvents.Where(e => e.IsActive && !e.IsBlocked && e.EventDate > DateTime.Now);

        if (!string.IsNullOrEmpty(query))
        {
            filteredEvents = filteredEvents.Where(e =>
                (e.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) == true) ||
                (e.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true) ||
                (e.Address?.Contains(query, StringComparison.OrdinalIgnoreCase) == true));
        }

        if (!string.IsNullOrEmpty(category))
        {
            filteredEvents = filteredEvents.Where(e => e.CategoryId == category);
        }

        if (date.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.EventDate.Date == date.Value.Date);
        }

        return filteredEvents.ToList();
    }

    public async Task<List<string>> GetAvailableCategoriesAsync()
    {
        var events = await _dataService.GetEventsAsync();
        // ИСПРАВЛЕНИЕ: Получаем все категории из будущих активных и незаблокированных событий
        return events?
            .Where(e => !string.IsNullOrEmpty(e.CategoryId) && e.IsActive && !e.IsBlocked && e.EventDate > DateTime.Now)
            .Select(e => e.CategoryId)
            .Distinct()
            .OrderBy(c => c)  // Сортируем для удобства
            .ToList() ?? new List<string>();
    }

    public async Task<List<Event>> GetFilteredEventsAsync(EventFilters filters)
    {
        var allEvents = await _dataService.GetEventsAsync();

        if (allEvents == null) return new List<Event>();

        var filteredEvents = allEvents.Where(e => e.IsActive && !e.IsBlocked);

        // Фильтр по категории
        if (!string.IsNullOrEmpty(filters.Category) && filters.Category != "Все категории")
        {
            filteredEvents = filteredEvents.Where(e => e.CategoryId == filters.Category);
        }

        // Фильтр по дате
        if (filters.Date.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.EventDate.Date == filters.Date.Value.Date);
        }

        // Фильтр по статусу участия
        if (!string.IsNullOrEmpty(filters.ParticipationStatus))
        {
            switch (filters.ParticipationStatus)
            {
                case "Есть свободные места":
                    filteredEvents = filteredEvents.Where(e => e.HasFreeSpots);
                    break;
                case "Я участвую":
                    // Здесь нужно добавить логику проверки участия текущего пользователя
                    break;
                case "Я не участвую":
                    // Логика для событий, где пользователь не участвует
                    break;
            }
        }

        // Фильтр по количеству участников
        if (!string.IsNullOrEmpty(filters.ParticipantCount))
        {
            switch (filters.ParticipantCount)
            {
                case "Мало участников (1-5)":
                    filteredEvents = filteredEvents.Where(e => e.ParticipantsCount >= 1 && e.ParticipantsCount <= 5);
                    break;
                case "Средняя группа (6-15)":
                    filteredEvents = filteredEvents.Where(e => e.ParticipantsCount >= 6 && e.ParticipantsCount <= 15);
                    break;
                case "Много участников (16+)":
                    filteredEvents = filteredEvents.Where(e => e.ParticipantsCount >= 16);
                    break;
            }
        }

        // Сортировка
        var eventsList = filteredEvents.ToList();
        if (!string.IsNullOrEmpty(filters.SortOption))
        {
            switch (filters.SortOption)
            {
                case "По дате (сначала новые)":
                    eventsList = eventsList.OrderByDescending(e => e.EventDate).ToList();
                    break;
                case "По дате (сначала старые)":
                    eventsList = eventsList.OrderBy(e => e.EventDate).ToList();
                    break;
                case "По количеству участников":
                    eventsList = eventsList.OrderByDescending(e => e.ParticipantsCount).ToList();
                    break;
                case "По расстоянию":
                    eventsList = eventsList.OrderBy(e => e.EventDate).ToList();
                    break;
            }
        }

        return eventsList;
    }
}