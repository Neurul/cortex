using CQRSlite.Commands;
using CQRSlite.Domain;
using System.Threading;
using System.Threading.Tasks;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
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
        private readonly ISession session;
        private readonly ILinkService linkService;

        public NeuronCommandHandlers(ISession session, ILinkService linkService)
        {
            this.session = session;
            this.linkService = linkService;
        }

        public async Task Handle(CreateNeuron message, CancellationToken token = default(CancellationToken))
        {
            var neuron = new Neuron(message.Id, message.Data);
            await this.session.Add(neuron, token);
            await this.session.Commit(token);
        }

        public async Task Handle(CreateNeuronWithTerminals message, CancellationToken token = default(CancellationToken))
        {
            var neuron = new Neuron(message.Id, message.Data);
            await neuron.AddTerminals(this.linkService, message.Terminals);
            await this.session.Add(neuron, token);
            await this.session.Commit(token);
        }

        public async Task Handle(ChangeNeuronData message, CancellationToken token = default(CancellationToken))
        {
            var neuron = await this.session.Get<Neuron>(message.Id, message.ExpectedVersion, token);
            neuron.ChangeData(message.NewData);
            await this.session.Commit(token);
        }

        public async Task Handle(AddTerminalsToNeuron message, CancellationToken token = default(CancellationToken))
        {
            var neuron = await this.session.Get<Neuron>(message.Id, message.ExpectedVersion, token);
            await neuron.AddTerminals(this.linkService, message.Terminals);
            await this.session.Commit(token);
        }

        public async Task Handle(RemoveTerminalsFromNeuron message, CancellationToken token = default(CancellationToken))
        {
            var neuron = await this.session.Get<Neuron>(message.Id, message.ExpectedVersion, token);
            neuron.RemoveTerminals(message.Terminals);
            await this.session.Commit(token);
        }

        public async Task Handle(DeactivateNeuron message, CancellationToken token = default(CancellationToken))
        {
            var neuron = await this.session.Get<Neuron>(message.Id, message.ExpectedVersion, token);
            neuron.Deactivate();
            await this.session.Commit(token);
        }
    }
}