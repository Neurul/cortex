using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using org.neurul.Common.Events;
using org.neurul.Common.Http;
using org.neurul.Cortex.Application.Neurons;
using org.neurul.Cortex.Domain.Model.Neurons;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.Events;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.Events.SQLite;
using System;

namespace org.neurul.Cortex.Port.Adapter.In.Api
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        public CustomBootstrapper()
        {
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
            // create a singleton instance which will be reused for all calls in current request
            var ipb = new Router();
            container.Register<ICommandSender, Router>(ipb);
            container.Register<IHandlerRegistrar, Router>(ipb);
            container.Register<IEventPublisher, Router>(ipb);
            container.Register<IEventSerializer, EventSerializer>();

            container.Register<INavigableEventStore, EventStore>();
            container.Register<NeuronCommandHandlers>();

            var ticl = new TinyIoCServiceLocator(container);
            container.Register<IServiceProvider, TinyIoCServiceLocator>(ticl);
            var registrar = new RouteRegistrar(ticl);
            registrar.Register(typeof(NeuronCommandHandlers));

            // Here we register our user mapper as a per-request singleton.
            // As this is now per-request we could inject a request scoped
            // database "context" or other request scoped services.
            ((TinyIoCServiceLocator)container.Resolve<IServiceProvider>()).SetRequestContainer(container);
            container.Register<IRepository>((x, y) => new Repository(x.Resolve<INavigableEventStore>()));
            container.Register<ISession, Session>();
        }
    }
}
