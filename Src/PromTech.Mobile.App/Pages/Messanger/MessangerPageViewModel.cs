using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PromTech.Mobile.Core.Interfaces;
using PromTech.Mobile.TCP.Interfaces;
using PromTech.Mobile.TCP.Model;

namespace PromTech.Mobile.App.Pages.Messanger
{
    public partial class MessangerPageViewModel : ObservableObject, IDisposable
    {
        public IAsyncRelayCommand SendCommand { get; private set; }

        [ObservableProperty]
        private string _messageText;

        [ObservableProperty]
        private string _sendText;

        private readonly ITcpClient _tcpService;
        private readonly ILocalStorage _localStorage;

        public MessangerPageViewModel(ITcpClient tcpClientService, ILocalStorage localStorage)
        {
            _tcpService = tcpClientService;
            _localStorage = localStorage;

            InitializeAsync();

            SendCommand = new AsyncRelayCommand(SendMessageAsync);

            // подписываемся на события
            _tcpService.OnConnectionStatusChanged += OnConnectionStatusChanged;
            _tcpService.OnMessageReceived += OnMessageReceived;
        }

        private async void InitializeAsync()
        {
            var connection = await _localStorage.LoadAsync();
            await _tcpService.StartAsync(connection.IPAddress, Convert.ToInt32(connection.Port));
        }

        /// <summary>
        /// Обработчик события получения сообщения
        /// </summary>
        /// <param name="obj"></param>
        private void OnMessageReceived(MessageContainer obj)
        {
            // не самый хороший вариант работы с логами
            switch (obj.MessageType)
            {
                case MessageType.Message:
                    MessageText += $"Ответ сервера : {obj.Message} \n";
                    break;

                case MessageType.Error:
                    // тут по хорошему еще логирование надо
                    MessageText += $"{obj.Message} \n";
                    break;
            }
        }

        /// <summary>
        /// Обработчик события изменения статуса подключения
        /// </summary>
        /// <param name="obj"></param>
        private void OnConnectionStatusChanged(bool obj)
        {
            MessageText += $"Состояние подключения : {obj} \n";
        }

        /// <summary>
        /// Отправка сообщения на сервер
        /// </summary>
        /// <returns></returns>
        private async Task SendMessageAsync()
        {
            if (!string.IsNullOrEmpty(SendText))
            {
                MessageText += $"Вы отправили: {SendText} \n";
                await _tcpService.SendMessageAsync(SendText);
                SendText = "";
            }
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            _tcpService.OnConnectionStatusChanged -= OnConnectionStatusChanged;
            _tcpService.OnMessageReceived -= OnMessageReceived;
            _tcpService.Stop();
            GC.SuppressFinalize(this);
        }
    }
}