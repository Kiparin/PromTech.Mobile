using PromTech.Mobile.TCP.Model;

namespace PromTech.Mobile.TCP.Interfaces
{
    public interface ITcpClient
    {
        /// <summary>
        /// Событие, возникающее при получении сообщения от сервера.
        /// </summary>
        event Action<MessageContainer> OnMessageReceived;

        /// <summary>
        /// Событие, возникающее при изменении состояния соединения.
        /// </summary>
        event Action<bool> OnConnectionStatusChanged;

        /// <summary>
        /// Проверка возможности подключения к серверу.
        /// </summary>
        /// <returns>True, если подключение возможно, иначе False.</returns>
        Task<MessageContainer> CheckConnectionAsync(string serverAddress, int port);

        /// <summary>
        /// Старт клиента Tcp соединения.
        /// </summary>
        Task StartAsync(string serverAddress, int port);

        /// <summary>
        /// Отправка сообщения на сервер.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        Task SendMessageAsync(string message);

        /// <summary>
        /// Останавливает клиент и закрывает соединение с сервером.
        /// </summary>
        void Stop();
    }
}