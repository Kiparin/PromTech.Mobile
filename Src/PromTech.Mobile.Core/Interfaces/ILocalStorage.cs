using PromTech.Mobile.Model;

namespace PromTech.Mobile.Core.Interfaces
{
    public interface ILocalStorage
    {
        /// <summary>
        /// Сохранение настроек подключения в Storage
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task SaveAsync(Connection connection);

        /// <summary>
        /// Получение настроек из Storage
        /// </summary>
        /// <returns></returns>
        Task<Connection> LoadAsync();
    }
}