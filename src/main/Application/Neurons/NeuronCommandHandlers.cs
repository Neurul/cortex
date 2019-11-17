using CQRSlite.Commands;
using CQRSlite.Domain;
using org.neurul.Common.Domain.Model;
using org.neurul.Common.Events;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using org.neurul.Cortex.Domain.Model.Users;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.Neurons
{
    public class NeuronCommandHandlers : 
        ICancellableCommandHandler<CreateNeuron>,
        ICancellableCommandHandler<ChangeNeuronTag>,
        ICancellableCommandHandler<DeactivateNeuron>
    {
        private readonly IEventSourceFactory eventSourceFactory;
        private readonly IUserRepository userRepository;
        private readonly ILayerPermitRepository layerPermitRepository;

        public NeuronCommandHandlers(IEventSourceFactory eventSourceFactory, IUserRepository userRepository, ILayerPermitRepository layerPermitRepository)
        {
            AssertionConcern.AssertArgumentNotNull(eventSourceFactory, nameof(eventSourceFactory));
            // TODO: Add TDD test
            AssertionConcern.AssertArgumentNotNull(userRepository, nameof(userRepository));
            AssertionConcern.AssertArgumentNotNull(layerPermitRepository, nameof(layerPermitRepository));

            this.eventSourceFactory = eventSourceFactory;
            this.userRepository = userRepository;
            this.layerPermitRepository = layerPermitRepository;
        }

        public async Task Handle(CreateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var eventSource = this.eventSourceFactory.CreateEventSource(message.AvatarId);
            Neuron neuron = null;

            // TODO: Add TDD test
            if (await eventSource.EventStore.CountNotifications() == 0)
                neuron = new Neuron(message.Id, message.Tag);
            else
            {
                await this.userRepository.Initialize(message.AvatarId);
                await this.layerPermitRepository.Initialize(message.AvatarId);
                // TODO: Add TDD test
                var author = await NeuronCommandHandlers.GetValidatedAuthorUser(message.SubjectId, eventSource.Session, this.userRepository, this.layerPermitRepository, token);
                Neuron layer = await eventSource.Session.GetOrDefaultIfGuidEmpty(
                    message.LayerId,
                    nameof(layer),
                    Neuron.RootLayerNeuron,
                    cancellationToken: token
                    );
                neuron = new Neuron(message.Id, message.Tag, layer, author);
            }

            await eventSource.Session.Add(neuron, token);
            await eventSource.Session.Commit(token);
        }

        internal static async Task<Author> GetValidatedAuthorUser(Guid subjectId, ISession session, IUserRepository userStore, ILayerPermitRepository layerPermitStore, CancellationToken token = default(CancellationToken))
        {
            User user = await userStore.GetBySubjectId(subjectId);
            // TODO: Add TDD test
            AssertionConcern.AssertStateTrue(user != null, Messages.Exception.UnauthorizedUserAccess);
            var permits = await layerPermitStore.GetAllByUserNeuronId(user.NeuronId);            
            var author = new Author(
                await session.Get<Neuron>(user.NeuronId, nameof(user), cancellationToken: token),
                user,
                permits
                );
            return author;
        }

        public async Task Handle(ChangeNeuronTag message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var eventSource = this.eventSourceFactory.CreateEventSource(message.AvatarId);

            await this.userRepository.Initialize(message.AvatarId);
            await this.layerPermitRepository.Initialize(message.AvatarId);

            // TODO: Add TDD test
            var author = await NeuronCommandHandlers.GetValidatedAuthorUser(message.SubjectId, eventSource.Session, this.userRepository, this.layerPermitRepository, token);
            Neuron neuron = await eventSource.Session.Get<Neuron>(message.Id, nameof(neuron), message.ExpectedVersion, token);
            Neuron layer = await eventSource.Session.GetOrDefaultIfGuidEmpty(
                    neuron.LayerId,
                    nameof(layer),
                    Neuron.RootLayerNeuron,
                    cancellationToken: token
                    );
            neuron.ChangeTag(message.NewTag, layer, author);

            await eventSource.Session.Commit(token);
        }

        public async Task Handle(DeactivateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            var eventSource = this.eventSourceFactory.CreateEventSource(message.AvatarId);
            await this.userRepository.Initialize(message.AvatarId);
            await this.layerPermitRepository.Initialize(message.AvatarId);

            // TODO: Add TDD test
            var author = await NeuronCommandHandlers.GetValidatedAuthorUser(message.SubjectId, eventSource.Session, this.userRepository, this.layerPermitRepository, token);
            Neuron neuron = await eventSource.Session.Get<Neuron>(message.Id, nameof(neuron), message.ExpectedVersion, token);
            Neuron layer = await eventSource.Session.GetOrDefaultIfGuidEmpty(
                    neuron.LayerId,
                    nameof(layer),
                    Neuron.RootLayerNeuron,
                    cancellationToken: token
                    );

            neuron.Deactivate(layer, author);
            await eventSource.Session.Commit(token);
        }
    }
}