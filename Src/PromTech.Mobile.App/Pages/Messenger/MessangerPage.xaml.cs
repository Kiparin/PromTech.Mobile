namespace PromTech.Mobile.App.Pages.Messenger;

public partial class MessengerPage : ContentPage
{
    public MessengerPage(MessengerPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}