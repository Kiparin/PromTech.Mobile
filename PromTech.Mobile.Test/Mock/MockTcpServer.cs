using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PromTech.Mobile.Test.Mock
{
    internal class MockTcpServer : IDisposable
    {
        private IPAddress _ipAddress;
        private int _port;

        private Encoding _encoder;
        private TcpListener _listener;
        private TcpClient _client;

        public MockTcpServer(IPAddress ipAddress, int port, string typeEncoding)
        {
            _ipAddress = ipAddress;
            _port = port;

            if (typeEncoding == "CP866")
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            _encoder = Encoding.GetEncoding(typeEncoding);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _listener = new TcpListener(_ipAddress, _port);
            _listener.Start();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_listener.Pending())
                    {
                        _client = await _listener.AcceptTcpClientAsync();
                        HandleClientAsync(_client);
                    }
                    else
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Сервер остановлен.");
            }
            finally
            {
                _listener.Stop();
            }
        }

        private async void HandleClientAsync(TcpClient client)
        {
            try
            {

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string request = _encoder.GetString(buffer, 0, bytesRead);
                    if (request == "Привет сервер!")
                    {
                        SendAync(client, "Правильно");
                    }
                    else
                    {
                        SendAync(client, request);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Соединение прервано.");
            }
            
        }

        private async void SendAync(TcpClient client, string message)
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = _encoder.GetBytes(message);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        public void Dispose()
        {
            _listener?.Stop();
            _client?.Close();

            GC.SuppressFinalize(this);
        }
    }
}