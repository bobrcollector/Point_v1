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
            filteredEvents = filteredEvents.Where(e => e.CategoryId == category || 
                                                      (e.CategoryIds != null && e.CategoryIds.Contains(category)));
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
        var categories = new List<string>();
        foreach (var e in events?.Where(e => e.IsActive && !e.IsBlocked && e.EventDate > DateTime.Now) ?? new List<Event>())
        {
            if (!string.IsNullOrEmpty(e.CategoryId))
            {
                categories.Add(e.CategoryId);
            }
            if (e.CategoryIds != null && e.CategoryIds.Count > 0)
            {
                categories.AddRange(e.CategoryIds);
            }
        }
        
        return categories.Distinct().OrderBy(c => c).ToList();
    }

    public async Task<List<Event>> GetFilteredEventsAsync(EventFilters filters)
    {
        var allEvents = await _dataService.GetEventsAsync();

        if (allEvents == null) return new List<Event>();

        var filteredEvents = allEvents.Where(e => e.IsActive && !e.IsBlocked);

        if (!string.IsNullOrEmpty(filters.Category) && filters.Category != "Все категории")
        {
            filteredEvents = filteredEvents.Where(e => e.CategoryId == filters.Category || 
                                                      (e.CategoryIds != null && e.CategoryIds.Contains(filters.Category)));
        }

        if (filters.Date.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.EventDate.Date == filters.Date.Value.Date);
        }

        if (!string.IsNullOrEmpty(filters.ParticipationStatus))
        {
            switch (filters.ParticipationStatus)
            {
                case "Есть свободные места":
                    filteredEvents = filteredEvents.Where(e => e.HasFreeSpots);
                    break;
                case "Я участвую":
                    break;
                case "Я не участвую":
                    break;
            }
        }
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