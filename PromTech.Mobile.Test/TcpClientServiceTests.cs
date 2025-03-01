using System.Net;

using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

using PromTech.Mobile.TCP.Model;
using PromTech.Mobile.TCP.Services;
using PromTech.Mobile.Test.Mock;

namespace PromTech.Mobile.Test.TcpTests
{
    public class TcpClientServiceTests
    {
        private readonly TcpClientService _tcpClientService;
        private MockTcpServer _mocTcpServer;
        private CancellationTokenSource _token;
        private MessageContainer _messageContainer;

        public TcpClientServiceTests()
        {
            _tcpClientService = new TcpClientService();
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldReturnErrorMessage_WhenConnectionIsFails()
        {
            var serverAddress = "127.0.0.1";
            var port = 8080;
            MakeMessageContainer("Не удалось подключиться к серверу: Tаймаут. Проверьте оборудование и попробуйте снова.", MessageType.Error);

            var result = await _tcpClientService.CheckConnectionAsync(serverAddress, port);

            Assert.NotNull(_messageContainer);
            Assert.Equal(_messageContainer.MessageType, result.MessageType);
            Assert.Equal(_messageContainer.Message, result.Message);
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldReturnErrorMessage_WhenEmptyAddress()
        {
            var serverAddress = "";
            var port = 0;
            MakeMessageContainer("Ошибка при проверке подключения: Невалидный адрес или порт.", MessageType.Error);

            var result = await _tcpClientService.CheckConnectionAsync(serverAddress, port);

            Assert.NotNull(_messageContainer);
            Assert.Equal(_messageContainer.MessageType, result.MessageType);
            Assert.Equal(_messageContainer.Message, result.Message);
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldReturnErrorMessage_WhenConnectionFails()
        {
            var serverAddress = "invalid.address";
            var port = 8080;

            MakeMessageContainer("Ошибка при проверке подключения: Этот хост неизвестен.", MessageType.Error);

            var result = await _tcpClientService.CheckConnectionAsync(serverAddress, port);

            Assert.NotNull(_messageContainer);
            Assert.Equal(_messageContainer.MessageType, result.MessageType);
            Assert.Equal(_messageContainer.Message, result.Message);
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldReturnErrorMessage_WhenConnectionTrue()
        {
            var serverAddress = "127.0.0.1";
            var port = 3000;

            MakeMessageContainer("Подключение к серверу возможно.", MessageType.Message);

            MakeConnectMokcServer("CP866");
            await Task.Delay(5000);

            var result = await _tcpClientService.CheckConnectionAsync(serverAddress, port);

            Assert.NotNull(_messageContainer);
            Assert.Equal(_messageContainer.MessageType, result.MessageType);
            Assert.Equal(_messageContainer.Message, result.Message);
            StopMokcServer();
        }

        [Fact]
        public async Task StartAsync_WhenServerNotConnected()
        {
            MakeMessageContainer(
                "Ошибка подключения: При попытке подключения к серверу возникла ошибка. Следующая попытка будет через 5 секунд",
                MessageType.Error);

            MessageContainer result = null;
            _tcpClientService.OnMessageReceived += (msg) => result = msg;

            bool? connectionStatus = null;
            _tcpClientService.OnConnectionStatusChanged += (status) => connectionStatus = status;

            StartTcpClient();
            await Task.Delay(5000);

            Assert.False(connectionStatus);
            Assert.NotNull(_messageContainer);
            Assert.Equal(_messageContainer.MessageType, result.MessageType);
            Assert.Equal(_messageContainer.Message, result.Message);
        }

        [Fact]
        public async Task StartAsync_WhenServerConnected()
        {

            bool? connectionStatus = null;
            _tcpClientService.OnConnectionStatusChanged += (status) => connectionStatus = status;
            MakeConnectMokcServer("CP866");

            StartTcpClient();
            await Task.Delay(5000);

            Assert.True(connectionStatus);
            StopMokcServer();
        }

        [Fact]
        public async Task SendMessageAsync_ShouldNotSend_WhenNotConnectedCP866()
        {
            var message = "Привет сервер!";
            MakeMessageContainer("Соединение с сервером не установлено.", MessageType.Error);

            MakeConnectMokcServer("CP866");

            MessageContainer result = null;
            _tcpClientService.OnMessageReceived += (msg) => result = msg;

            await _tcpClientService.SendMessageAsync(message);

            Assert.NotNull(_messageContainer);
            Assert.Equal(_messageContainer.MessageType, result.MessageType);
            Assert.Equal(_messageContainer.Message, result.Message);
            StopMokcServer();
        }

        [Fact]
        public async Task SendMessageAsync_ShouldSend_TrueReceivedCP866()
        {
            var message = "Привет сервер!";
            MakeMessageContainer("Правильно", MessageType.Message);

            MakeConnectMokcServer("CP866");
            StartTcpClient();
            await Task.Delay(5000);

            MessageContainer result = null;
            _tcpClientService.OnMessageReceived += (msg) => result = msg;

            await _tcpClientService.SendMessageAsync(message);
            await Task.Delay(8000);

            Assert.NotNull(_messageContainer);
            Assert.Equal(_messageContainer.MessageType, result.MessageType);
            Assert.Equal(_messageContainer.Message, result.Message);
            StopMokcServer();
        }

        [Fact]
        public async Task SendMessageAsync_ShouldSend_CheckCP866Encoder()
        {
            var message = "Привет сервер!";
            MakeMessageContainer("?????? ??????!", MessageType.Message);

            MakeConnectMokcServer("ASCII");
            StartTcpClient();
            await Task.Delay(5000);

            MessageContainer result = null;
            _tcpClientService.OnMessageReceived += (msg) => result = msg;

            await _tcpClientService.SendMessageAsync(message);
            await Task.Delay(10000);

            Assert.NotNull(_messageContainer);
            Assert.Equal(_messageContainer.MessageType, result.MessageType);
            Assert.Equal(_messageContainer.Message, result.Message);
            StopMokcServer();
        }

        [Fact]
        public void Stop_ShouldCloseConnectionAndInvokeStatusChanged()
        {
            bool? connectionStatus = null;
            _tcpClientService.OnConnectionStatusChanged += (status) => connectionStatus = status;

            _tcpClientService.Stop();

            Assert.False(connectionStatus);
        }

        private async void MakeConnectMokcServer(string typeEncoding)
        {
            _mocTcpServer = new MockTcpServer(IPAddress.Parse("127.0.0.1"), 3000, typeEncoding);
            _token = new CancellationTokenSource();

            var task = _mocTcpServer.StartAsync(_token.Token);
        }

        private async void StopMokcServer()
        {
            _token.Cancel();
            await Task.Delay(5000);
            _mocTcpServer?.Dispose();
        }

        private void MakeMessageContainer(string message, MessageType type)
        {
            _messageContainer = new MessageContainer
            {
                MessageType = type,
                Message = message
            };
        }

        private async void StartTcpClient()
        {
            var serverAddress = "127.0.0.1";
            var port = 3000;
            await _tcpClientService.StartAsync(serverAddress, port);
        }
    }
}