using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class EditProfilePage : ContentPage
{
    public EditProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}