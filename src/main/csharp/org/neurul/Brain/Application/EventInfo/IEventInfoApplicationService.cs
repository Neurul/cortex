using org.neurul.Common.Events;
using System.Threading.Tasks;

namespace org.neurul.Brain.Application.EventInfo
{
    public interface IEventInfoApplicationService
    {
        Task<EventInfoLog> GetCurrentEventInfoLog();

        Task<EventInfoLog> GetEventInfoLog(string eventInfoLogId);
    }
}
