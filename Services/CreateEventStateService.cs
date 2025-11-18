using Point_v1.Models;
using Point_v1.ViewModels;

namespace Point_v1.Services;

public static class CreateEventStateService
{
    public static string Title { get; set; }
    public static string Description { get; set; }
    public static Interest SelectedInterest { get; set; }
    public static DateTime EventDate { get; set; }
    public static TimeSpan EventTime { get; set; }
    public static int MaxParticipants { get; set; }
    public static string Address { get; set; }
    public static double? Latitude { get; set; }
    public static double? Longitude { get; set; }

    public static bool HasState => !string.IsNullOrEmpty(Title) || 
                                    !string.IsNullOrEmpty(Description) || 
                                    SelectedInterest != null ||
                                    !string.IsNullOrEmpty(Address);

    public static void SaveState(CreateEventViewModel viewModel)
    {
        Title = viewModel.Title;
        Description = viewModel.Description;
        SelectedInterest = viewModel.SelectedInterest;
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

