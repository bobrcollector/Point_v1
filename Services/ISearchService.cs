using Point_v1.Models;

namespace Point_v1.Services;

public interface ISearchService
{
    Task<List<Event>> SearchEventsAsync(string searchText, string selectedCategory, DateTime? selectedDate);
    Task<List<Event>> FilterEventsByInterestsAsync(List<string> interestIds);
    Task<List<Event>> GetEventsNearbyAsync(double latitude, double longitude, double radiusKm);
}

public class SearchService : ISearchService
{
    private readonly IDataService _dataService;

    public SearchService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<List<Event>> SearchEventsAsync(string searchText, string selectedCategory, DateTime? selectedDate)
    {
        try
        {
            var allEvents = await _dataService.GetEventsAsync();

            if (allEvents == null || !allEvents.Any())
                return new List<Event>();

            var filteredEvents = allEvents.AsEnumerable();

            // Фильтрация по тексту поиска
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredEvents = filteredEvents.Where(e =>
                    e.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    e.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    e.Address.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            // Фильтрация по категории
            if (!string.IsNullOrWhiteSpace(selectedCategory))
            {
                filteredEvents = filteredEvents.Where(e =>
                    e.CategoryId.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            // Фильтрация по дате
            if (selectedDate.HasValue)
            {
                filteredEvents = filteredEvents.Where(e =>
                    e.EventDate.Date == selectedDate.Value.Date);
            }

            return filteredEvents.ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка поиска событий: {ex.Message}");
            return new List<Event>();
        }
    }

    public async Task<List<Event>> FilterEventsByInterestsAsync(List<string> interestIds)
    {
        try
        {
            var allEvents = await _dataService.GetEventsAsync();

            if (allEvents == null || !allEvents.Any() || interestIds == null || !interestIds.Any())
                return allEvents ?? new List<Event>();

            return allEvents.Where(e => interestIds.Contains(e.CategoryId)).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка фильтрации по интересам: {ex.Message}");
            return new List<Event>();
        }
    }

    public async Task<List<Event>> GetEventsNearbyAsync(double latitude, double longitude, double radiusKm)
    {
        // Заглушка для геопоиска - в реальном приложении здесь будет работа с координатами
        // Пока возвращаем все события
        return await _dataService.GetEventsAsync();
    }
}