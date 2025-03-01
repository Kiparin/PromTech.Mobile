using System.Net.Sockets;
using System.Text;

using PromTech.Mobile.TCP.Interfaces;
using PromTech.Mobile.TCP.Model;

namespace PromTech.Mobile.TCP.Services
{
    public class TcpClientService : ITcpClient
    {
        public event Action<MessageContainer> OnMessageReceived;

        public event Action<bool> OnConnectionStatusChanged;

        private readonly Encoding _encoder;
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cancellationTokenSource;

        private string _serverAddress;
        private int _port;
        private bool _isConnected = false;

        public TcpClientService()
        {
            //Регистрируем кодировку для работы с cp866
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoder = Encoding.GetEncoding("CP866");
        }

        public async Task<MessageContainer> CheckConnectionAsync(string serverAddress, int port)
        {
            var message = new MessageContainer();
            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    //Устанавливаем таймаут 2 секунды
                    cts.CancelAfter(2000);

                    using (var testClient = new TcpClient())
                    {
                        await testClient.ConnectAsync(serverAddress, port, cts.Token);

                        message.MessageType = MessageType.Message;
                        message.Message = "Подключение к серверу возможно.";
                        return message;
                    }
                }
                catch (OperationCanceledException)
                {
                    message.MessageType = MessageType.Error;
                    message.Message = "Не удалось подключиться к серверу: таймаут. Проверьте оборудование и попробуйте снова.";
                    return message;
                }
                catch (Exception ex)
                {
                    message.MessageType = MessageType.Error;
                    message.Message = $"Ошибка при проверке подключения: {ex.Message}";
                    return message;
                }
            }
        }

        public async Task StartAsync(string address, int ip)
        {
            _serverAddress = address;
            _port = ip;

            if (_client == null)
            {
                _client = new TcpClient();
            }
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await ConnectToServerAsync();

                //Запускаем Мониторинг подключения и Слушаем сервер параллельно
                await Task.WhenAll(
                    MonitorConnectionAsync(_cancellationTokenSource.Token),
                    ListenForServerResponsesAsync(_cancellationTokenSource.Token)
                );
            }
            catch (Exception ex)
            {
                await Error($"Ошибка при подключении к серверу: {ex.Message}");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_stream == null || !_isConnected)
            {
                var error = "Соединение с сервером не установлено.";

                Console.WriteLine(error);
                await MessageReсiver(error, MessageType.Error);
                return;
            }

            try
            {
                var data = _encoder.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                await Error($"Ошибка при отправке сообщения: {ex.Message}");
            }
        }

        /// <summary>
        /// Подключение к серверу и инициализация потока для обмена данными.
        /// </summary>
        private async Task ConnectToServerAsync()
        {
            if (_client == null)
            {
                _client = new TcpClient();
            }

            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(5000);

                try
                {
                    await _client.ConnectAsync(_serverAddress, _port, cts.Token);
                    _stream = _client.GetStream();
                    await ConnectionStatusUpdate(true);
                }
                catch (ObjectDisposedException ex)
                {
                    _stream?.Close();
                    _stream = null;
                    _client?.Close();
                    _client = null;
                    throw new Exception("Отчистка данных и попытка переподключения через 5 секунд");
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка чтения данных. Следующая попытка будет через 5 секунд");
                }
            }
        }

        /// <summary>
        /// Отслеживает состояние соединения и пытается переподключиться при его потере.
        /// </summary>
        /// <param name="cancellationToken">Токен для отмены задачи.</param>
        private async Task MonitorConnectionAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!_isConnected)
                    {
                        Console.WriteLine("Переподключение");
                        await ConnectToServerAsync();
                        await Task.WhenAll(
                            ListenForServerResponsesAsync(_cancellationTokenSource.Token)
                        );
                    }
                }
                catch (Exception e)
                {
                    await Error($"Ошибка переподключения: {e.Message}");
                }

                await Task.Delay(5000, cancellationToken);
            }
        }

        /// <summary>
        /// Прослушивает ответы от сервера и уведомляет о полученных сообщениях.
        /// </summary>
        /// <param name="cancellationToken">Токен для отмены задачи.</param>
        private async Task ListenForServerResponsesAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead > 0)
                    {
                        var receivedMessage = _encoder.GetString(buffer, 0, bytesRead);
                        await MessageReсiver(receivedMessage, MessageType.Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                await Error("Прослушка ответов от сервера остановлена.");
            }
            catch (Exception ex)
            {
                await Error($"Ошибка при прослушке ответов от сервера: {ex.Message}");
            }
        }

        /// <summary>
        /// Концентратор ошибок.
        /// </summary>
        /// <param name="error">Текст ошибки.</param>
        /// <returns></returns>
        private async Task Error(string error)
        {
            Console.WriteLine(error);
            await ConnectionStatusUpdate(false);
            await MessageReсiver(error, MessageType.Error);
        }

        /// <summary>
        /// Обновление статуса соединения.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private Task ConnectionStatusUpdate(bool status)
        {
            _isConnected = status;
            OnConnectionStatusChanged?.Invoke(status);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Отправка сообщений всем подписчикам.
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        /// <param name="type">Тип сообщения.</param>
        /// <returns></returns>
        private Task MessageReсiver(string message, MessageType type)
        {
            var container = new MessageContainer();

            container.Message = message;
            container.MessageType = type;
            OnMessageReceived?.Invoke(container);

            return Task.CompletedTask;
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _stream?.Close();
            _client?.Close();
            _isConnected = false;
            OnConnectionStatusChanged?.Invoke(_isConnected);
        }
    }
}