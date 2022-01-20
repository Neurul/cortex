using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using neurUL.Common.Http;
using neurUL.Cortex.Application.Neurons.Commands;
using neurUL.Cortex.Domain.Model.Neurons;
using System.Threading;
using System.Threading.Tasks;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.In;
using System.Linq;
using CQRSlite.Events;
using CQRSlite.Domain;

namespace neurUL.Cortex.Application.Neurons
{
    public class NeuronCommandHandlers : 
        ICancellableCommandHandler<CreateNeuron>,
        ICancellableCommandHandler<DeactivateNeuron>
    {
        private readonly IAuthoredEventStore eventStore;
        private readonly ISession session;

        public NeuronCommandHandlers(IEventStore eventStore, ISession session)
        {
            AssertionConcern.AssertArgumentNotNull(eventStore, nameof(eventStore));
            AssertionConcern.AssertArgumentValid(
                es => es is IAuthoredEventStore,
                eventStore,
                "Specified 'eventStore' must be an IAuthoredEventStore implementation.",
                nameof(eventStore)
                );
            AssertionConcern.AssertArgumentNotNull(session, nameof(session));

            this.eventStore = (IAuthoredEventStore)eventStore;
            this.session = session;
        }

        public async Task Handle(CreateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            this.eventStore.SetAuthor(message.AuthorId);

            var neuron = new Neuron(message.Id);

            await this.session.Add(neuron, token);
            await this.session.Commit(token);
        }

        public async Task Handle(DeactivateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            this.eventStore.SetAuthor(message.AuthorId);
            Neuron neuron = await this.session.Get<Neuron>(message.Id, nameof(neuron), message.ExpectedVersion, token);
            neuron.Deactivate();
            await this.session.Commit(token);
        }
    }
}