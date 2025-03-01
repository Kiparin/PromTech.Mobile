namespace PromTech.Mobile.App.Pages.Messanger;

public partial class MessangerPage : ContentPage
{
    public MessangerPage(MessangerPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}