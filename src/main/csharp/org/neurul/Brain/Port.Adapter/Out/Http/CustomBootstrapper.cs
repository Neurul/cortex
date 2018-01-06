using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using org.neurul.Brain.Application.EventInfo;
using org.neurul.Brain.Port.Adapter.IO.Persistence.Events;
using org.neurul.Brain.Port.Adapter.IO.Persistence.Events.SQLite;
using org.neurul.Common.Events;
using System;
using System.IO;

namespace org.neurul.Brain.Port.Adapter.Out.Http
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        private static readonly string DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Events.db");
        
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var ipb = new Router();
            container.Register<IEventPublisher, Router>(ipb);
            container.Register<IEventSerializer, EventSerializer>();
            container.Register<INavigableEventStore, EventStore>(
                new EventStore(
                    CustomBootstrapper.DatabasePath,
                    container.Resolve<IEventSerializer>(),
                    container.Resolve<IEventPublisher>()
                )
            );

            container.Resolve<INavigableEventStore>().Initialize();
            container.Register<IEventInfoApplicationService, EventInfoApplicationService>();
        }
    }
}
