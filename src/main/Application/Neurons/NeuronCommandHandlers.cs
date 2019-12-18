using CQRSlite.Commands;
using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using System.Threading;
using System.Threading.Tasks;
using works.ei8.EventSourcing.Client;
using works.ei8.EventSourcing.Client.In;

namespace org.neurul.Cortex.Application.Neurons
{
    public class NeuronCommandHandlers : 
        ICancellableCommandHandler<CreateNeuron>,
        ICancellableCommandHandler<DeactivateNeuron>
    {
        private readonly IEventSourceFactory eventSourceFactory;
        private readonly ISettingsService settingsService;

        public NeuronCommandHandlers(IEventSourceFactory eventSourceFactory, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(eventSourceFactory, nameof(eventSourceFactory));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.eventSourceFactory = eventSourceFactory;
            this.settingsService = settingsService;
        }

        public async Task Handle(CreateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var eventSource = this.eventSourceFactory.Create(
                Helper.GetAvatarUrl(this.settingsService.EventSourcingInBaseUrl, message.AvatarId),
                Helper.GetAvatarUrl(this.settingsService.EventSourcingOutBaseUrl, message.AvatarId),
                message.AuthorId
                );

            // Neuron neuron = null;
            // TODO: Transfer as a Domain.Model.Validator in Sentry, which ensures that message.Id == message.AuthorId when avatar is empty
            // Add TDD test
            //if ((await eventSource.NotificationClient.GetNotificationLog(string.Empty, token)).TotalCount == 0)
            //    neuron = new Neuron(message.Id);
            //else

            var neuron = new Neuron(message.Id);
            
            await eventSource.Session.Add(neuron, token);
            await eventSource.Session.Commit(token);
        }

        public async Task Handle(DeactivateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var eventSource = this.eventSourceFactory.Create(
                Helper.GetAvatarUrl(this.settingsService.EventSourcingInBaseUrl, message.AvatarId),
                Helper.GetAvatarUrl(this.settingsService.EventSourcingOutBaseUrl, message.AvatarId),
                message.AuthorId
                );
            Neuron neuron = await eventSource.Session.Get<Neuron>(message.Id, nameof(neuron), message.ExpectedVersion, token);

            neuron.Deactivate();
            await eventSource.Session.Commit(token);
        }
    }
}