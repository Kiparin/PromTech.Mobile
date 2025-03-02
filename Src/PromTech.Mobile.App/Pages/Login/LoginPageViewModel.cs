using System.ComponentModel.DataAnnotations;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PromTech.Mobile.App.Pages.Messenger;
using PromTech.Mobile.App.Resources.Localization;
using PromTech.Mobile.App.Services;
using PromTech.Mobile.Core.Interfaces;
using PromTech.Mobile.Model;
using PromTech.Mobile.TCP.Interfaces;
using PromTech.Mobile.TCP.Model;

namespace PromTech.Mobile.App.Pages.Login
{
    public partial class LoginPageViewModel : ObservableValidator
    {
        public IAsyncRelayCommand ConnectCommand { get; private set; }

        [ObservableProperty]
        private bool _isLoadingEnabled = false;

        [ObservableProperty]
        private bool _isAuthEnabled = true;

        [ObservableProperty]
        [Required(ErrorMessage = "IP-адрес обязателен")]
        [RegularExpression(@"^(?:(?:[0-9]{1,3}\.){3}[0-9]{1,3}|(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,})$", ErrorMessage = "Неверный формат IP-адреса")]
        private string _ipAddress;

        [ObservableProperty]
        [Required(ErrorMessage = "Порт обязателен")]
        [Range(1, 65535, ErrorMessage = "Порт должен быть в диапазоне 1-65535")]
        private string _port;

        private readonly ILocalStorage _localStorage;
        private readonly ITcpClient _tcpClient;
        private Connection connection = new();

        public LoginPageViewModel(ILocalStorage localStorage, ITcpClient tcpClient)
        {
            _localStorage = localStorage;
            _tcpClient = tcpClient;

            SetValue();

            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
        }

        private void SetValue()
        {
            connection = _localStorage.LoadAsync().Result;

            IpAddress = connection.IPAddress;
            Port = connection.Port.ToString();
        }

        private async Task ConnectAsync()
        {
            await IsVisibleElement(true,false);

            if (ValidateInput())
            {
                await IsVisibleElement(false, true);
                return;
            }

            try
            {
                //проверяем возможность подключения
                var result = await _tcpClient.CheckConnectionAsync(IpAddress, Convert.ToInt32(Port));
                if (result.MessageType == MessageType.Error)
                {
                    await AlertService.Show(result.Message);
                    await IsVisibleElement(false, true);
                }
                else
                {
                    connection.IPAddress = IpAddress;
                    connection.Port = Port;
                    await _localStorage.SaveAsync(connection);
                    NavigateTo();
                    await IsVisibleElement(false, true);
                }
            }
            catch
            {
                await AlertService.Show(TextLocalize.UnknownError);
                await IsVisibleElement(false, true);
            }
        }

        private void NavigateTo()
        {
            // Этом моменте нужно было бы использовать сервис навигации для перехода на другую страницу
            // Но в данном случае, я просто создаю новую страницу и устанавливаю ее в качестве главной
            var messagerPage = IPlatformApplication.Current.Services.GetService<MessengerPage>();
            Application.Current.MainPage = new NavigationPage(messagerPage);
        }

        private Task IsVisibleElement(bool isLoadingEnable, bool isAuthEnable)
        {
            IsLoadingEnabled = isLoadingEnable;
            IsAuthEnabled = isAuthEnable;

            return Task.CompletedTask;
        }

        //Валидация введенных данных
        private bool ValidateInput()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                AlertService.Show(string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage)));
            }

            return HasErrors;
        }
    }
}