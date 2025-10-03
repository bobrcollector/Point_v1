using Point_v1.ViewModels;

namespace Point_v1.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnLoginTapped(object sender, EventArgs e)
    {
        if (BindingContext is AuthViewModel viewModel)
        {
            viewModel.IsRegisterMode = false;
        }
    }
}