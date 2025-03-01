namespace PromTech.Mobile.App.Pages.Login;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}