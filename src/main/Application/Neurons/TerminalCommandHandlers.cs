using CQRSlite.Commands;
using CQRSlite.Domain;
using org.neurul.Common.Domain.Model;
using org.neurul.Common.Events;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.Neurons
{
    public class TerminalCommandHandlers :
        ICancellableCommandHandler<CreateTerminal>,
        ICancellableCommandHandler<DeactivateTerminal>
    {
        private readonly INavigableEventStore eventStore;
        private readonly ISession session;

        public TerminalCommandHandlers(INavigableEventStore eventStore, ISession session)
        {
            AssertionConcern.AssertArgumentNotNull(eventStore, nameof(eventStore));
            AssertionConcern.AssertArgumentNotNull(session, nameof(session));

            this.eventStore = eventStore;
            this.session = session;
        }

        public async Task Handle(CreateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            await this.eventStore.Initialize(message.AvatarId);

            Neuron presynaptic = await this.session.Get<Neuron>(message.PresynapticNeuronId, nameof(presynaptic), cancellationToken: token),
                postsynaptic = await this.session.Get<Neuron>(message.PostsynapticNeuronId, nameof(postsynaptic), cancellationToken: token),
                author = await this.session.Get<Neuron>(message.AuthorId, nameof(author), cancellationToken: token);

            var terminal = new Terminal(message.Id, presynaptic, postsynaptic,
                message.Effect, message.Strength, author);
            await this.session.Add(terminal, token);
            await this.session.Commit(token);
        }

        public async Task Handle(DeactivateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            await this.eventStore.Initialize(message.AvatarId);

            Neuron author = await this.session.Get<Neuron>(message.AuthorId, nameof(author), cancellationToken: token);
            Terminal terminal = await this.session.Get<Terminal>(message.Id, nameof(terminal), message.ExpectedVersion, token);

            terminal.Deactivate(author);
            await this.session.Commit(token);
        }
    }
}
