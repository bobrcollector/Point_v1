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
        await SeedSampleEventsAsync(); // ��������� ��� ������
    }

    private async Task SeedInterestsAsync()
    {
        var interests = new List<Interest>
        {
            new Interest { Name = "D&D" },
            new Interest { Name = "�����" },
            new Interest { Name = "�������" },
            new Interest { Name = "�������" },
            new Interest { Name = "���������� ����" },
            new Interest { Name = "�������" },
            new Interest { Name = "���������" },
            new Interest { Name = "����������������" },
            new Interest { Name = "����������" },
            new Interest { Name = "������" },
            new Interest { Name = "�����" },
            new Interest { Name = "�����" },
            new Interest { Name = "���������" },
            new Interest { Name = "�����������" },
            new Interest { Name = "�����" }
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
            Title = "����� ���������� ��� � ����������",
            Description = "������ � �����, ���������, ���������. ��������� ���� ����! ������� � ������� ���������������.",
            CategoryId = "���������� ����",
            Address = "��������� 'Space', ��. �����������, 15",
            EventDate = DateTime.Now.AddDays(1).AddHours(19),
            CreatorId = "sample_user_1",
            ParticipantIds = new List<string> { "sample_user_1", "sample_user_2" }
        },
        new Event
        {
            Title = "�����-�������: ������",
            Description = "������� ������ ������������ ����� ������. ��������� ������� � ������� ����������!",
            CategoryId = "�����",
            Address = "�������� 'Geek Room', ��. ������, 28",
            EventDate = DateTime.Now.AddDays(3).AddHours(17),
            CreatorId = "sample_user_2",
            ParticipantIds = new List<string> { "sample_user_2", "sample_user_3" }
        },
        new Event
        {
            Title = "������� �� ��������� �������",
            Description = "������ ������� digital art � Photoshop. ��������� �������� � ����������� ��������.",
            CategoryId = "���������",
            Address = "������ 'ArtSpace', ��. ����������, 7",
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