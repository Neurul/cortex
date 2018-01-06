using CQRSlite.Caching;
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using org.neurul.Brain.Application.Neurons;
using org.neurul.Brain.Domain.Model.Neurons;
using org.neurul.Brain.Port.Adapter.IO.Persistence.Events;
using org.neurul.Brain.Port.Adapter.IO.Persistence.Events.SQLite;
using org.neurul.Common.Http;
using System;
using System.IO;
using org.neurul.Common.Events;

namespace org.neurul.Brain.Port.Adapter.In.Http
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        private static readonly string DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Events.db");
        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            // Here we register our user mapper as a per-request singleton.
            // As this is now per-request we could inject a request scoped
            // database "context" or other request scoped services.
            ((TinyIoCServiceLocator)container.Resolve<IServiceProvider>()).SetRequestContainer(container);

            container.Register<ICache, MemoryCache>();
            container.Register<IRepository>(
                (x, y) => 
                new CacheRepository(
                    new Repository(x.Resolve<INavigableEventStore>()), 
                    x.Resolve<INavigableEventStore>(), 
                    x.Resolve<ICache>()
                    )
                );
            container.Register<ISession, Session>();
        }
        
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var ipb = new Router();
            container.Register<ICommandSender, Router>(ipb);
            container.Register<IHandlerRegistrar, Router>(ipb);
            container.Register<IEventPublisher, Router>(ipb);
            container.Register<IEventSerializer, EventSerializer>();
            container.Register<INavigableEventStore, EventStore>(
                new EventStore(
                    CustomBootstrapper.DatabasePath,
                    container.Resolve<IEventSerializer>(),
                    container.Resolve<IEventPublisher>()
                )
            );

            container.Register<ILinkService, LinkService>();

            container.Resolve<INavigableEventStore>().Initialize();

            // TODO: Scan for commandhandlers and eventhandlers
            //services.Scan(scan => scan
            //    .FromAssemblies(typeof(InventoryCommandHandlers).GetTypeInfo().Assembly)
            //        .AddClasses(classes => classes.Where(x => {
            //            var allInterfaces = x.GetInterfaces();
            //            return
            //                allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(ICommandHandler<>)) ||
            //                allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(IEventHandler<>));
            //        }))
            //        .AsSelf()
            //        .WithTransientLifetime()
            //);
            container.Register<NeuronCommandHandlers>();

            var ticl = new TinyIoCServiceLocator(container);
            container.Register<IServiceProvider, TinyIoCServiceLocator>(ticl);
            var registrar = new RouteRegistrar(ticl);
            registrar.Register(typeof(NeuronCommandHandlers));
        }
    }
}
