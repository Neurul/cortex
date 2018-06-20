using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using org.neurul.Common.Events;
using org.neurul.Cortex.Application.Notification;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.Events;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.Events.SQLite;

namespace org.neurul.Cortex.Port.Adapter.Out.Api
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        public CustomBootstrapper()
        {
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            var ipb = new Router();
            container.Register<IEventPublisher, Router>(ipb);
            container.Register<IEventSerializer, EventSerializer>();
            container.Register<INavigableEventStore, EventStore>();
            container.Register<INotificationApplicationService, NotificationApplicationService>();
        }
    }
}
