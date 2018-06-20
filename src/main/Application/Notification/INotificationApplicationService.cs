using org.neurul.Common.Events;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.Notification
{
    public interface INotificationApplicationService
    {
        Task<NotificationLog> GetCurrentNotificationLog(string storeId);

        Task<NotificationLog> GetNotificationLog(string storeId, string notificationLogId);
    }
}
