using CQRSlite.Commands;
using CQRSlite.Domain;
using org.neurul.Common.Domain.Model;
using org.neurul.Common.Events;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.Neurons
{
    public class NeuronCommandHandlers : 
        ICancellableCommandHandler<CreateNeuron>,
        ICancellableCommandHandler<CreateAuthorNeuron>,
        ICancellableCommandHandler<ChangeNeuronTag>,
        ICancellableCommandHandler<DeactivateNeuron>
    {
        private readonly INavigableEventStore eventStore;
        private readonly ISession session;

        public NeuronCommandHandlers(INavigableEventStore eventStore, ISession session)
        {
            AssertionConcern.AssertArgumentNotNull(eventStore, nameof(eventStore));
            AssertionConcern.AssertArgumentNotNull(session, nameof(session));

            this.eventStore = eventStore;
            this.session = session;
        }

        public async Task Handle(CreateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            await this.eventStore.Initialize(message.AvatarId);
            
            Neuron author = await this.session.Get<Neuron>(message.AuthorId, nameof(author), cancellationToken: token),
                neuron = new Neuron(message.Id, message.Tag, author);
            
            await this.session.Add(neuron, token);
            await this.session.Commit(token);
        }

        public async Task Handle(CreateAuthorNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));
            
            await this.eventStore.Initialize(message.AvatarId);
            
            var neuron = new Neuron(message.Id, message.Tag);

            await this.session.Add(neuron, token);
            await this.session.Commit(token);
        }

        public async Task Handle(ChangeNeuronTag message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            await this.eventStore.Initialize(message.AvatarId);

            Neuron neuron = await this.session.Get<Neuron>(message.Id, nameof(neuron), message.ExpectedVersion, token),
                author = await this.session.Get<Neuron>(message.AuthorId, nameof(author), cancellationToken: token);
            neuron.ChangeTag(message.NewTag, author);

            await this.session.Commit(token);
        }

        public async Task Handle(DeactivateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            await this.eventStore.Initialize(message.AvatarId);

            Neuron neuron = await this.session.Get<Neuron>(message.Id, nameof(neuron), message.ExpectedVersion, token),
                author = await this.session.Get<Neuron>(message.AuthorId, nameof(author), cancellationToken: token);
            
            neuron.Deactivate(author);
            await this.session.Commit(token);
        }
    }
}