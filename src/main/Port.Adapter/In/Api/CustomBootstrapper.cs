﻿using CQRSlite.Commands;
using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Common.Http;
using neurUL.Cortex.Application;
using neurUL.Cortex.Application.Neurons;
using neurUL.Cortex.Port.Adapter.IO.Process.Services;
using System;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.In;
using CQRSlite.Domain;
using neurUL.Cortex.Port.Adapter.In.InProcess;

namespace neurUL.Cortex.Port.Adapter.In.Api
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
            container.Register<IEventSerializer, EventSerializer>();
            container.Register<ISettingsService, SettingsService>();
            container.Register<IEventStoreUrlService>(
                (tic, npo) => {
                    var ss = container.Resolve<ISettingsService>();
                    return new EventStoreUrlService(
                        ss.EventSourcingInBaseUrl + "/",
                        ss.EventSourcingOutBaseUrl + "/"
                        );
                });
            container.Register<IEventStore, HttpEventStoreClient>();
            container.Register<IRepository, Repository>();
            container.Register<ISession, Session>();
            container.Register<INeuronAdapter, NeuronAdapter>();
            container.Register<ITerminalAdapter, TerminalAdapter>();
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
