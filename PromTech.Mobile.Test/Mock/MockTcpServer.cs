using System.Net.Sockets;
using System.Net;
using System.Text;

namespace PromTech.Mobile.Test.Mock
{
    class MockTcpServer
    {
        private IPAddress _ipAddress;
        private int _port;
        private Encoding _encoder;

        public MockTcpServer(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoder = Encoding.GetEncoding("CP866");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            TcpListener listener = new TcpListener(_ipAddress, _port);
            listener.Start();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (listener.Pending())
                    {
                        TcpClient client = await listener.AcceptTcpClientAsync();
                        HandleClientAsync(client); 
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
                listener.Stop();
            }
        }

        private async void HandleClientAsync(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string request = _encoder.GetString(buffer, 0, bytesRead);
                if(request == "Hello, server!")
                {
                    string response = "Правильно";
                    byte[] responseBytes = _encoder.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine("Ответ отправлен.");
                }
                else
                {
                    string response = "Неправильно";
                    byte[] responseBytes = _encoder.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine("Ответ отправлен.");
                }
               
            }
        }
    }
}
