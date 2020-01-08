using CQRSlite.Events;
using CQRSlite.Routing;
using Nancy;
using Nancy.TinyIoc;
using org.neurul.Cortex.Application;
using org.neurul.Cortex.Application.Neurons;
using org.neurul.Cortex.Port.Adapter.IO.Process.Services;
using works.ei8.EventSourcing.Client;

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

            container.Register<IEventSerializer, EventSerializer>(new EventSerializer());
            container.Register<IEventSourceFactory, EventSourceFactory>();
            container.Register<ISettingsService, SettingsService>();
            container.Register<INeuronQueryService, NeuronQueryService>();
        }
    }
}
