using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace PromTech.Mobile.App.Services
{
    public sealed class AlertService
    {
        /// <summary>
        /// Отобразить сообщение типа Toast.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Task Show(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Short);
            toast.Show();

            return Task.CompletedTask;
        }
    }
}