using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using org.neurul.Cortex.Application.EventInfo;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.Events;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.Events.SQLite;
using org.neurul.Common.Events;
using System;
using System.IO;
using System.Collections.Generic;
using Nancy.Bootstrapper;
using System.Linq;

namespace org.neurul.Cortex.Port.Adapter.Out.Http
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

        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                return GetType().Assembly.GetTypes().Where(type => type.BaseType == typeof(NancyModule)).Select(type => new ModuleRegistration(type));
            }
        }
    }
}
