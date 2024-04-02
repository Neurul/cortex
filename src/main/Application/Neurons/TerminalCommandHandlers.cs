using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using ei8.EventSourcing.Client;
using ei8.EventSourcing.Client.In;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Application.Neurons.Commands;
using neurUL.Cortex.Domain.Model.Neurons;
using System.Threading;
using System.Threading.Tasks;

namespace neurUL.Cortex.Application.Neurons
{
    public class TerminalCommandHandlers :
        ICancellableCommandHandler<CreateTerminal>,
        ICancellableCommandHandler<DeactivateTerminal>
    {
        private readonly IAuthoredEventStore eventStore;
        private readonly ISession session;

        public TerminalCommandHandlers(IEventStore eventStore, ISession session)
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

        public async Task Handle(CreateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            this.eventStore.SetAuthor(message.AuthorId);

            var terminal = new Terminal(message.Id, message.PresynapticNeuronId, message.PostsynapticNeuronId, message.Effect, message.Strength);
            await this.session.Add(terminal, token);
            await this.session.Commit(token);
        }

        public async Task Handle(DeactivateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            this.eventStore.SetAuthor(message.AuthorId);

            Terminal terminal = await this.session.Get<Terminal>(message.Id, nameof(terminal), message.ExpectedVersion, token);
            
            terminal.Deactivate();
            await this.session.Commit(token);
        }
    }
}
