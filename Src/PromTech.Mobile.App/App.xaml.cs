using PromTech.Mobile.App.Pages.Login;

namespace PromTech.Mobile.App
{
    public partial class App : Application
    {
        public App(LoginPage page)
        {
            InitializeComponent();

            MainPage = page;
        }
    }
}