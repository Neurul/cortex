using CQRSlite.Domain;
using CQRSlite.Events;
using ei8.EventSourcing.Client;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Cortex.Application;
using neurUL.Cortex.Application.Neurons;
using neurUL.Cortex.Port.Adapter.IO.Process.Services;

namespace neurUL.Cortex.Port.Adapter.Out.Api
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        public CustomBootstrapper()
        {
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.Register<IEventSerializer, EventSerializer>(new EventSerializer());
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
            container.Register<INeuronQueryService, NeuronQueryService>();
        }
    }
}
