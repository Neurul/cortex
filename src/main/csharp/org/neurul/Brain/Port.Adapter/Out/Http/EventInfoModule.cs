using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using org.neurul.Brain.Application.EventInfo;
using org.neurul.Common;
using org.neurul.Common.Events;
using System.Linq;
using System.Text;

namespace org.neurul.Brain.Port.Adapter.Out.Http
{
    public class EventInfoModule : NancyModule
    {
        public EventInfoModule(IEventInfoApplicationService eventService) : base("/brain")
        {
            this.Get("/events", async (parameters) => this.ProcessLog(
                await eventService.GetCurrentEventInfoLog(), 
                this.Request.Url.ToString()
                )
                );

            this.Get("/events/{logid}", async (parameters) => this.ProcessLog(
                await eventService.GetEventInfoLog(parameters.logid),
                this.Request.Url.ToString().Substring(
                    0, 
                    this.Request.Url.ToString().Length - parameters.logid.ToString().Length - 1
                    )
                )
                );
        }

        private TextResponse ProcessLog(EventInfoLog log, string requestUrlBase)
        {
            var response = new TextResponse(JsonConvert.SerializeObject(log.EventInfoList.ToArray()));
            var sb = new StringBuilder();
            ResponseHelper.Header.Link.AppendValue(
                sb,
                $"{requestUrlBase}/{log.EventInfoLogId}", 
                Common.Constants.Response.Header.Link.Relation.Self
                );

            if (log.HasFirstEventInfoLog)
                ResponseHelper.Header.Link.AppendValue(
                    sb,
                    $"{requestUrlBase}/{log.FirstEventInfoLogId}",
                    Common.Constants.Response.Header.Link.Relation.First
                    );

            if (log.HasPreviousEventInfoLog)
                ResponseHelper.Header.Link.AppendValue(
                    sb,
                    $"{requestUrlBase}/{log.PreviousEventInfoLogId}", 
                    Common.Constants.Response.Header.Link.Relation.Previous
                    );

            if (log.HasNextEventInfoLog)
                ResponseHelper.Header.Link.AppendValue(
                    sb,
                    $"{requestUrlBase}/{log.NextEventInfoLogId}", 
                    Common.Constants.Response.Header.Link.Relation.Next
                    );

            response.Headers.Add(Common.Constants.Response.Header.Link.Key, sb.ToString());
            return response;
        }
    }
}