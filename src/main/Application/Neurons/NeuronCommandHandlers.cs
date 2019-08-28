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
        private readonly INavigableEventStore eventStore;
        private readonly ISession session;
        private readonly IUserRepository userRepository;
        private readonly ILayerPermitRepository layerPermitRepository;

        public NeuronCommandHandlers(INavigableEventStore eventStore, ISession session, IUserRepository userRepository, ILayerPermitRepository layerPermitRepository)
        {
            AssertionConcern.AssertArgumentNotNull(eventStore, nameof(eventStore));
            AssertionConcern.AssertArgumentNotNull(session, nameof(session));
            // TODO: Add TDD test
            AssertionConcern.AssertArgumentNotNull(userRepository, nameof(userRepository));
            AssertionConcern.AssertArgumentNotNull(layerPermitRepository, nameof(layerPermitRepository));

            this.eventStore = eventStore;
            this.session = session;
            this.userRepository = userRepository;
            this.layerPermitRepository = layerPermitRepository;
        }

        public async Task Handle(CreateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            await this.eventStore.Initialize(message.AvatarId);
            Neuron neuron = null;

            // TODO: Add TDD test
            if (await this.eventStore.CountNotifications() == 0)
                neuron = new Neuron(message.Id, message.Tag);
            else
            {
                await this.userRepository.Initialize(message.AvatarId);
                await this.layerPermitRepository.Initialize(message.AvatarId);
                // TODO: Add TDD test
                var author = await NeuronCommandHandlers.GetValidatedAuthorUser(message.SubjectId, session, this.userRepository, this.layerPermitRepository, token);
                Neuron layer = await this.session.GetOrDefaultIfGuidEmpty(
                    message.LayerId,
                    nameof(layer),
                    Neuron.RootLayerNeuron,
                    cancellationToken: token
                    );
                neuron = new Neuron(message.Id, message.Tag, layer, author);
            }

            await this.session.Add(neuron, token);
            await this.session.Commit(token);
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

            await this.eventStore.Initialize(message.AvatarId);
            await this.userRepository.Initialize(message.AvatarId);
            await this.layerPermitRepository.Initialize(message.AvatarId);

            // TODO: Add TDD test
            var author = await NeuronCommandHandlers.GetValidatedAuthorUser(message.SubjectId, session, this.userRepository, this.layerPermitRepository, token);
            Neuron neuron = await this.session.Get<Neuron>(message.Id, nameof(neuron), message.ExpectedVersion, token);
            Neuron layer = await this.session.GetOrDefaultIfGuidEmpty(
                    neuron.LayerId,
                    nameof(layer),
                    Neuron.RootLayerNeuron,
                    cancellationToken: token
                    );
            neuron.ChangeTag(message.NewTag, layer, author);

            await this.session.Commit(token);
        }

        public async Task Handle(DeactivateNeuron message, CancellationToken token = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(message, nameof(message));

            await this.eventStore.Initialize(message.AvatarId);
            await this.userRepository.Initialize(message.AvatarId);
            await this.layerPermitRepository.Initialize(message.AvatarId);

            // TODO: Add TDD test
            var author = await NeuronCommandHandlers.GetValidatedAuthorUser(message.SubjectId, session, this.userRepository, this.layerPermitRepository, token);
            Neuron neuron = await this.session.Get<Neuron>(message.Id, nameof(neuron), message.ExpectedVersion, token);
            Neuron layer = await this.session.GetOrDefaultIfGuidEmpty(
                    neuron.LayerId,
                    nameof(layer),
                    Neuron.RootLayerNeuron,
                    cancellationToken: token
                    );

            neuron.Deactivate(layer, author);
            await this.session.Commit(token);
        }
    }
}