using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using org.neurul.Cortex.Application.Notification;
using org.neurul.Common;
using org.neurul.Common.Events;
using System.Linq;
using System.Text;

namespace org.neurul.Cortex.Port.Adapter.Out.Api
{
    public class NotificationModule : NancyModule
    {
        public NotificationModule(INotificationApplicationService eventService) : base("/{avatarId}/cortex/notifications")
        {
            this.Get("/", async (parameters) => this.ProcessLog(
                await eventService.GetCurrentNotificationLog(parameters.avatarId), 
                this.Request.Url.ToString()
                )
                );

            this.Get("/{logid}", async (parameters) => this.ProcessLog(
                await eventService.GetNotificationLog(parameters.avatarId, parameters.logid),
                this.Request.Url.ToString().Substring(
                    0, 
                    this.Request.Url.ToString().Length - parameters.logid.ToString().Length - 1
                    )
                )
                );
        }

        private TextResponse ProcessLog(NotificationLog log, string requestUrlBase)
        {
            var response = new TextResponse(JsonConvert.SerializeObject(log.NotificationList.ToArray()));
            var sb = new StringBuilder();
            ResponseHelper.Header.Link.AppendValue(
                sb,
                $"{requestUrlBase}/{log.NotificationLogId}", 
                org.neurul.Common.Constants.Response.Header.Link.Relation.Self
                );

            if (log.HasFirstNotificationLog)
                ResponseHelper.Header.Link.AppendValue(
                    sb,
                    $"{requestUrlBase}/{log.FirstNotificationLogId}",
                    org.neurul.Common.Constants.Response.Header.Link.Relation.First
                    );

            if (log.HasPreviousNotificationLog)
                ResponseHelper.Header.Link.AppendValue(
                    sb,
                    $"{requestUrlBase}/{log.PreviousNotificationLogId}", 
                    org.neurul.Common.Constants.Response.Header.Link.Relation.Previous
                    );

            if (log.HasNextNotificationLog)
                ResponseHelper.Header.Link.AppendValue(
                    sb,
                    $"{requestUrlBase}/{log.NextNotificationLogId}", 
                    org.neurul.Common.Constants.Response.Header.Link.Relation.Next
                    );

            response.Headers.Add(org.neurul.Common.Constants.Response.Header.Link.Key, sb.ToString());
            return response;
        }
    }
}