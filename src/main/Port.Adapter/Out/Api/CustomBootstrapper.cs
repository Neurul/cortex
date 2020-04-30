using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using neurUL.Cortex.Application;
using neurUL.Cortex.Application.Neurons;
using neurUL.Cortex.Port.Adapter.IO.Process.Services;
using ei8.EventSourcing.Client;

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
            container.Register<IEventSourceFactory, EventSourceFactory>();
            container.Register<ISettingsService, SettingsService>();
            container.Register<INeuronQueryService, NeuronQueryService>();
        }
    }
}
