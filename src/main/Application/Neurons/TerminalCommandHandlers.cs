using CQRSlite.Commands;
using CQRSlite.Domain;
using org.neurul.Common.Domain.Model;
using org.neurul.Common.Events;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using org.neurul.Cortex.Domain.Model.Users;
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
        private readonly IEventSourceFactory eventSourceFactory;
        private readonly IUserRepository userRepository;
        private readonly ILayerPermitRepository layerPermitRepository;

        public TerminalCommandHandlers(IEventSourceFactory eventSourceFactory, IUserRepository userRepository, ILayerPermitRepository layerPermitRepository)
        {
            AssertionConcern.AssertArgumentNotNull(eventSourceFactory, nameof(eventSourceFactory));
            // TODO: Add TDD test
            AssertionConcern.AssertArgumentNotNull(userRepository, nameof(userRepository));
            AssertionConcern.AssertArgumentNotNull(layerPermitRepository, nameof(layerPermitRepository));

            this.eventSourceFactory = eventSourceFactory;
            this.userRepository = userRepository;
            this.layerPermitRepository = layerPermitRepository;
        }

        public async Task Handle(CreateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var eventSource = this.eventSourceFactory.CreateEventSource(message.AvatarId);
            await this.userRepository.Initialize(message.AvatarId);
            await this.layerPermitRepository.Initialize(message.AvatarId);

            Neuron presynaptic = await eventSource.Session.Get<Neuron>(message.PresynapticNeuronId, nameof(presynaptic), cancellationToken: token),
                postsynaptic = await eventSource.Session.Get<Neuron>(message.PostsynapticNeuronId, nameof(postsynaptic), cancellationToken: token),
                presynapticLayer = await eventSource.Session.GetOrDefaultIfGuidEmpty(
                    presynaptic.LayerId, 
                    nameof(presynapticLayer), 
                    Neuron.RootLayerNeuron, 
                    cancellationToken: token
                    );

            // TODO: Add TDD test
            var author = await NeuronCommandHandlers.GetValidatedAuthorUser(message.SubjectId, eventSource.Session, this.userRepository, this.layerPermitRepository, token);

            var terminal = new Terminal(message.Id, presynaptic, presynapticLayer, postsynaptic, message.Effect, message.Strength, author);
            await eventSource.Session.Add(terminal, token);
            await eventSource.Session.Commit(token);
        }

        public async Task Handle(DeactivateTerminal message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var eventSource = this.eventSourceFactory.CreateEventSource(message.AvatarId);
            await this.userRepository.Initialize(message.AvatarId);
            await this.layerPermitRepository.Initialize(message.AvatarId);

            // TODO: Add TDD test
            var author = await NeuronCommandHandlers.GetValidatedAuthorUser(message.SubjectId, eventSource.Session, this.userRepository, this.layerPermitRepository, token);
            Terminal terminal = await eventSource.Session.Get<Terminal>(message.Id, nameof(terminal), message.ExpectedVersion, token);
            Neuron presynaptic = await eventSource.Session.Get<Neuron>(terminal.PresynapticNeuronId, nameof(presynaptic), cancellationToken: token);
            Neuron presynapticLayer = await eventSource.Session.GetOrDefaultIfGuidEmpty(
                    presynaptic.LayerId,
                    nameof(presynapticLayer),
                    Neuron.RootLayerNeuron,
                    cancellationToken: token
                    );

            terminal.Deactivate(presynaptic, presynapticLayer, author);
            await eventSource.Session.Commit(token);
        }
    }
}
