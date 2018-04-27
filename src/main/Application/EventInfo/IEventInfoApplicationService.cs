using org.neurul.Common.Events;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.EventInfo
{
    public interface IEventInfoApplicationService
    {
        Task<EventInfoLog> GetCurrentEventInfoLog(string storeId);

        Task<EventInfoLog> GetEventInfoLog(string storeId, string eventInfoLogId);
    }
}
