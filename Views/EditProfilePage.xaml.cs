using Point_v1.ViewModels;
using Point_v1.Models;

namespace Point_v1.Views;

public partial class EditProfilePage : ContentPage
{
    public EditProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is ProfileViewModel viewModel)
        {
            await viewModel.LoadAllInterestsForEdit();
        }
    }
    
    private void OnInterestClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Interest interest)
        {
            if (BindingContext is ProfileViewModel viewModel)
            {
                viewModel.ToggleInterestCommand.Execute(interest);
            }
        }
    }
}