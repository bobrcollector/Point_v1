using Point_v1.Models;

namespace Point_v1.Services;

public interface ISearchService
{
    Task<List<Event>> SearchEventsAsync(string query, string category, DateTime? date);
    Task<List<string>> GetAvailableCategoriesAsync();
    Task<List<Event>> GetFilteredEventsAsync(EventFilters filters);
}