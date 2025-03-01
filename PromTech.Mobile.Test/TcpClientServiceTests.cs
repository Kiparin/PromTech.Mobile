using System.Net;

using PromTech.Mobile.TCP.Model;
using PromTech.Mobile.TCP.Services;
using PromTech.Mobile.Test.Mock;

namespace PromTech.Mobile.Test
{
    public class TcpClientServiceTests
    {
        private readonly TcpClientService _tcpClientService;
        private MockTcpServer _mocTcpServer;
        private CancellationTokenSource _token;

        public TcpClientServiceTests()
        {
            _tcpClientService = new TcpClientService();
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldReturnErrorMessage_WhenConnectionIsFails()
        {
            var serverAddress = "127.0.0.1";
            var port = 8080;
            var expectedMessage = new MessageContainer
            {
                MessageType = MessageType.Error,
                Message = "Не удалось подключиться к серверу: таймаут. Проверьте оборудование и попробуйте снова."
            };

            var result = await _tcpClientService.CheckConnectionAsync(serverAddress, port);

            Assert.Equal(expectedMessage.MessageType, result.MessageType);
            Assert.Equal(expectedMessage.Message, result.Message);
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldReturnErrorMessage_WhenConnectionFails()
        {
            var serverAddress = "invalid.address";
            var port = 8080;
            var expectedMessage = new MessageContainer
            {
                MessageType = MessageType.Error,
                Message = "Ошибка при проверке подключения: Этот хост неизвестен."
            };

            var result = await _tcpClientService.CheckConnectionAsync(serverAddress, port);

            Assert.Equal(expectedMessage.MessageType, result.MessageType);
            Assert.Equal(expectedMessage.Message, result.Message);
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldReturnErrorMessage_WhenConnectionTrue()
        {
            var expectedMessage = new MessageContainer
            {
                MessageType = MessageType.Message,
                Message = "Подключение к серверу возможно."
            };
            var serverAddress = "127.0.0.1";
            var port = 3000;

            MakeConnectMokcServer("CP866");

            await Task.Delay(5000);

            var result = await _tcpClientService.CheckConnectionAsync(serverAddress, port);

            Assert.Equal(expectedMessage.MessageType, result.MessageType);
            Assert.Equal(expectedMessage.Message, result.Message);
            StopMokcServer();
        }

        [Fact]
        public async Task SendMessageAsync_ShouldNotSend_WhenNotConnectedCP866()
        {
            var message = "Привет сервер!";

            MakeConnectMokcServer("CP866");

            string receivedErrorMessage = null;
            _tcpClientService.OnMessageReceived += (msg) => receivedErrorMessage = msg.Message;

            await _tcpClientService.SendMessageAsync(message);
            Assert.Contains("Соединение с сервером не установлено.", receivedErrorMessage);
            StopMokcServer();
        }

        [Fact]
        public async Task SendMessageAsync_ShouldSend_TrueReceivedCP866()
        {
            var message = "Привет сервер!";

            MakeConnectMokcServer("CP866");
            StartTcpClient();
            await Task.Delay(5000);

            string receivedMessage = null;
            _tcpClientService.OnMessageReceived += (msg) => receivedMessage = msg.Message;

            await _tcpClientService.SendMessageAsync(message);
            await Task.Delay(8000);

            Assert.Contains("Правильно", receivedMessage);
            StopMokcServer();
        }

        [Fact]
        public async Task SendMessageAsync_ShouldSend_CheckCP866Encoder()
        {
            var message = "Привет сервер!";

            MakeConnectMokcServer("ASCII");
            StartTcpClient();
            await Task.Delay(5000);

            string receivedMessage = null;
            _tcpClientService.OnMessageReceived += (msg) => receivedMessage = msg.Message;

            await _tcpClientService.SendMessageAsync(message);
            await Task.Delay(10000);

            Assert.Contains("?????? ??????!", receivedMessage);
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

        private async void StartTcpClient()
        {
            var serverAddress = "127.0.0.1";
            var port = 3000;
            await _tcpClientService.StartAsync(serverAddress, port);
        }
    }
}