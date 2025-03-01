using PromTech.Mobile.Core.Interfaces;
using PromTech.Mobile.Model;

namespace PromTech.Mobile.Core.Services.LocalStorage
{
    //// Можно было сереализовать объект в JSON и сохранить сереализацию
    //// но решил оставить так.

    public class PreferenceLocalStorage : ILocalStorage
    {
        public Task<Connection> LoadAsync()
        {
            var connection = new Connection();
            connection.IPAddress = Preferences.Default.Get("ip_address", "");
            connection.Port = Preferences.Default.Get("port", "");

            return Task.FromResult(connection);
        }

        public Task SaveAsync(Connection connection)
        {
            Preferences.Default.Set("ip_address", connection.IPAddress);
            Preferences.Default.Set("port", connection.Port);

            return Task.CompletedTask;
        }
    }
}