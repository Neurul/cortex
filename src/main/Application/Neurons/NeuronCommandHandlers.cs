using CQRSlite.Commands;
using CQRSlite.Domain;
using System.Threading;
using System.Threading.Tasks;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using org.neurul.Common.Events;
using org.neurul.Common.Domain.Model;
using System;
using System.Linq;

namespace org.neurul.Cortex.Application.Neurons
{
    public class NeuronCommandHandlers : 
        ICancellableCommandHandler<CreateNeuron>,
        ICancellableCommandHandler<CreateNeuronWithTerminals>,
        ICancellableCommandHandler<ChangeNeuronData>,
        ICancellableCommandHandler<AddTerminalsToNeuron>,
        ICancellableCommandHandler<RemoveTerminalsFromNeuron>,
        ICancellableCommandHandler<DeactivateNeuron>
    {
        private readonly INavigableEventStore eventStore;
        private readonly ISession session;

        public NeuronCommandHandlers(INavigableEventStore eventStore, ISession session)
        {
            this.eventStore = eventStore;
            this.session = session;
        }

        private static async Task AssertAuthorExistsAndActive(string authorId, CancellationToken token, ISession session)
        {
            var author = await session.Get<Neuron>(Guid.Parse(authorId), cancellationToken: token);
            AssertionConcern.AssertArgumentTrue(author.Active, $"Specified Author id '{authorId}' is not active.");
        }

        public async Task Handle(CreateNeuron message, CancellationToken token = default(CancellationToken))
        {
            await this.eventStore.Initialize(message.AvatarId);

            if (message.AuthorId != message.Id.ToString())
                await NeuronCommandHandlers.AssertAuthorExistsAndActive(message.AuthorId, token, session);

            var neuron = new Neuron(message.Id, message.Data, message.AuthorId);
            await this.session.Add(neuron, token);
            await this.session.Commit(token);
        }        

        public async Task Handle(CreateNeuronWithTerminals message, CancellationToken token = default(CancellationToken))
        {
            await this.eventStore.Initialize(message.AvatarId);

            if (message.AuthorId != message.Id.ToString())
                await NeuronCommandHandlers.AssertAuthorExistsAndActive(message.AuthorId, token, session);

            var neuron = new Neuron(message.Id, message.Data, message.AuthorId);
            await neuron.AddTerminals(message.Terminals, message.AuthorId);
            await this.session.Add(neuron, token);
            await this.session.Commit(token);
        }

        public async Task Handle(ChangeNeuronData message, CancellationToken token = default(CancellationToken))
        {
            await this.eventStore.Initialize(message.AvatarId);

            await NeuronCommandHandlers.AssertAuthorExistsAndActive(message.AuthorId, token, session);

            var neuron = await this.session.Get<Neuron>(message.Id, message.ExpectedVersion, token);
            neuron.ChangeData(message.NewData, message.AuthorId);
            await this.session.Commit(token);
        }

        public async Task Handle(AddTerminalsToNeuron message, CancellationToken token = default(CancellationToken))
        {
            await this.eventStore.Initialize(message.AvatarId);

            await NeuronCommandHandlers.AssertAuthorExistsAndActive(message.AuthorId, token, session);

            var neuron = await this.session.Get<Neuron>(message.Id, message.ExpectedVersion, token);
            await neuron.AddTerminals(message.Terminals, message.AuthorId);
            await this.session.Commit(token);
        }

        public async Task Handle(RemoveTerminalsFromNeuron message, CancellationToken token = default(CancellationToken))
        {
            await this.eventStore.Initialize(message.AvatarId);

            await NeuronCommandHandlers.AssertAuthorExistsAndActive(message.AuthorId, token, session);

            var neuron = await this.session.Get<Neuron>(message.Id, message.ExpectedVersion, token);
            neuron.RemoveTerminals(message.TargetIds, message.AuthorId);
            await this.session.Commit(token);
        }

        public async Task Handle(DeactivateNeuron message, CancellationToken token = default(CancellationToken))
        {
            await this.eventStore.Initialize(message.AvatarId);

            await NeuronCommandHandlers.AssertAuthorExistsAndActive(message.AuthorId, token, session);

            var neuron = await this.session.Get<Neuron>(message.Id, message.ExpectedVersion, token);
            neuron.Deactivate(message.AuthorId);
            await this.session.Commit(token);
        }
    }
}