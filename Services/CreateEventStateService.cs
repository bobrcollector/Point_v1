using Point_v1.Models;
using Point_v1.ViewModels;

namespace Point_v1.Services;

public static class CreateEventStateService
{
    public static string Title { get; set; }
    public static string Description { get; set; }
    public static Interest SelectedInterest { get; set; }
    public static List<Interest> SelectedInterests { get; set; } = new List<Interest>();
    public static DateTime EventDate { get; set; }
    public static TimeSpan EventTime { get; set; }
    public static int MaxParticipants { get; set; }
    public static string Address { get; set; }
    public static double? Latitude { get; set; }
    public static double? Longitude { get; set; }

    public static bool HasState => !string.IsNullOrEmpty(Title) || 
                                    !string.IsNullOrEmpty(Description) || 
                                    (SelectedInterests != null && SelectedInterests.Count > 0) ||
                                    SelectedInterest != null ||
                                    !string.IsNullOrEmpty(Address);

    public static void SaveState(CreateEventViewModel viewModel)
    {
        Title = viewModel.Title;
        Description = viewModel.Description;
        SelectedInterest = viewModel.SelectedInterest;
        SelectedInterests = viewModel.SelectedInterests != null ? new List<Interest>(viewModel.SelectedInterests) : new List<Interest>();
        EventDate = viewModel.EventDate;
        EventTime = viewModel.EventTime;
        MaxParticipants = viewModel.MaxParticipants;
        Address = viewModel.Address;
        Latitude = viewModel.Latitude;
        Longitude = viewModel.Longitude;
    }

    public static void RestoreState(CreateEventViewModel viewModel)
    {
        if (!HasState) return;

        viewModel.Title = Title;
        viewModel.Description = Description;
        viewModel.SelectedInterest = SelectedInterest;
        if (SelectedInterests != null && SelectedInterests.Count > 0)
        {
            viewModel.SelectedInterests = new List<Interest>(SelectedInterests);
            if (viewModel.Interests != null)
            {
                foreach (var interest in viewModel.Interests)
                {
                    interest.IsSelected = SelectedInterests.Any(si => si.Id == interest.Id || si.Name == interest.Name);
                }
            }
        }
        viewModel.EventDate = EventDate;
        viewModel.EventTime = EventTime;
        viewModel.MaxParticipants = MaxParticipants;
        viewModel.Address = Address;
        viewModel.Latitude = Latitude;
        viewModel.Longitude = Longitude;
    }

    public static void Clear()
    {
        Title = null;
        SelectedInterests = new List<Interest>();
        Description = null;
        SelectedInterest = null;
        EventDate = DateTime.Today.AddDays(1);
        EventTime = TimeSpan.FromHours(19);
        MaxParticipants = 20;
        Address = null;
        Latitude = null;
        Longitude = null;
    }
}

