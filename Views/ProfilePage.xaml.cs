using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnEditProfileClicked(object sender, EventArgs e)
    {
        DisplayAlert("��������������", "������� �������������� ������� ����� �������� �����", "OK");
    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        DisplayAlert("���������", "��������� ���������� ����� �������� �����", "OK");
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfileViewModel viewModel)
        {
            await viewModel.LoadUserData();
            if (viewModel.IsAuthenticated)
            {
                _ = viewModel.LoadUserStatistics(viewModel.GetCurrentUserId());
            }
        }
    }
}