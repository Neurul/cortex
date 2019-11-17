using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using org.neurul.Common.Events;
using org.neurul.Common.Http;
using org.neurul.Cortex.Application.Neurons;
using org.neurul.Cortex.Domain.Model.Users;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.Events;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.Events.SQLite;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.IdentityAccess;
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
            container.Register<IEventSerializer, EventSerializer>(new EventSerializer());
            container.Register<IUserRepository, UserRepository>();
            container.Register<ILayerPermitRepository, LayerPermitRepository>();
            container.Register<IEventSourceFactory, EventSourceFactory>();
            container.Register<NeuronCommandHandlers>();

            var ticl = new TinyIoCServiceLocator(container);
            container.Register<IServiceProvider, TinyIoCServiceLocator>(ticl);
            var registrar = new RouteRegistrar(ticl);
            registrar.Register(typeof(NeuronCommandHandlers));

            // Here we register our user mapper as a per-request singleton.
            // As this is now per-request we could inject a request scoped
            // database "context" or other request scoped services.
            ((TinyIoCServiceLocator)container.Resolve<IServiceProvider>()).SetRequestContainer(container);
        }
    }
}
