using Point_v1.Models;

namespace Point_v1.Services;

public class DatabaseSeeder
{
    private readonly IDataService _dataService;

    public DatabaseSeeder(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task SeedInitialDataAsync()
    {
        await SeedInterestsAsync();
        await SeedSampleEventsAsync(); // Добавляем эту строку
    }

    private async Task SeedInterestsAsync()
    {
        var interests = new List<Interest>
        {
            new Interest { Name = "D&D" },
            new Interest { Name = "Аниме" },
            new Interest { Name = "Комиксы" },
            new Interest { Name = "Косплей" },
            new Interest { Name = "Настольные игры" },
            new Interest { Name = "Встречи" },
            new Interest { Name = "Искусство" },
            new Interest { Name = "Программирование" },
            new Interest { Name = "Фотография" },
            new Interest { Name = "Музыка" },
            new Interest { Name = "Танцы" },
            new Interest { Name = "Спорт" },
            new Interest { Name = "Кулинария" },
            new Interest { Name = "Путешествия" },
            new Interest { Name = "Книги" }
        };

        foreach (var interest in interests)
        {
            await _dataService.AddInterestAsync(interest);
        }
    }
    public async Task SeedSampleEventsAsync()
    {
        var events = new List<Event>
    {
        new Event
        {
            Title = "Вечер настольных игр в коворкинге",
            Description = "Играем в Мафию, Каркассон, Монополию. Приносите свои игры! Напитки и закуски предоставляются.",
            CategoryId = "Настольные игры",
            Address = "Коворкинг 'Space', ул. Центральная, 15",
            EventDate = DateTime.Now.AddDays(1).AddHours(19),
            CreatorId = "sample_user_1",
            ParticipantIds = new List<string> { "sample_user_1", "sample_user_2" }
        },
        new Event
        {
            Title = "Аниме-марафон: Наруто",
            Description = "Смотрим вместе классические серии Наруто. Приносите попкорн и хорошее настроение!",
            CategoryId = "Аниме",
            Address = "Антикафе 'Geek Room', пр. Победы, 28",
            EventDate = DateTime.Now.AddDays(3).AddHours(17),
            CreatorId = "sample_user_2",
            ParticipantIds = new List<string> { "sample_user_2", "sample_user_3" }
        },
        new Event
        {
            Title = "Воркшоп по цифровому рисунку",
            Description = "Учимся основам digital art в Photoshop. Приносите ноутбуки и графические планшеты.",
            CategoryId = "Искусство",
            Address = "Студия 'ArtSpace', ул. Творческая, 7",
            EventDate = DateTime.Now.AddDays(5).AddHours(15),
            CreatorId = "sample_user_3",
            ParticipantIds = new List<string> { "sample_user_3" }
        }
    };

        foreach (var eventItem in events)
        {
            await _dataService.AddEventAsync(eventItem);
        }
    }
}