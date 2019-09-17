using CQRSlite.Domain;
using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Domain.Model.Users;
using System;
using System.Linq;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    /// <summary>
    /// Represents a Neuron.
    /// TODO: [Must] Decouple Neuron from concepts of Active, Tag, Author, User, and Layer. Perhaps transfer these to other microservices.
    /// </summary>
    public class Neuron : AssertiveAggregateRoot
    {
        public static readonly Neuron RootLayerNeuron = Neuron.CreateRootLayerNeuron();

        // TODO: Add TDD test
        private static Neuron CreateRootLayerNeuron()
        {
            var result = new Neuron();
            result.Construct(Guid.Empty, "Root Layer", result, Guid.Empty, true);
            return result;
        }

        private Neuron() { }

        /// <summary>
        /// Constructs an Author Neuron. Only called when creating the first Neuron in an Avatar.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tag"></param>
        public Neuron(Guid id, string tag)
        {
            // TODO: Add TDD test
            this.Construct(id, tag, Neuron.RootLayerNeuron, id, false);
        }

        /// <summary>
        /// Constructs a Neuron.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tag"></param>
        /// <param name="layerId"></param>
        /// <param name="authorNeuron">Author Neuron used to indicate author of creation event.</param>
        /// <param name="authorUser">User matched with Subject Id from User database.</param>
        public Neuron(Guid id, string tag, Neuron layer, Author author)
        {
            AssertionConcern.AssertArgumentValid(g => g != author.Neuron.Id, id, Messages.Exception.NonAuthorConstructor, nameof(id));
            // TODO: Add TDD test
            AssertionConcern.AssertArgumentNotNull(layer, nameof(layer));

            // TODO: Add TDD test
            Neuron.ValidateAuthorAccess(author.Neuron.Id, tag, layer, author);

            this.Construct(id, tag, layer, author.Neuron.Id, false);
        }

        private void Construct(Guid id, string tag, Neuron layer, Guid authorId, bool isConstructingRootLayer)
        {
            // TODO: Add TDD test
            AssertionConcern.AssertArgumentValid(i => isConstructingRootLayer || id != Guid.Empty, id, Messages.Exception.IdEmpty, nameof(id));
            AssertionConcern.AssertArgumentNotNull(tag, nameof(tag));
            // TODO: Add TDD test
            AssertionConcern.AssertArgumentNotNull(layer, nameof(layer));

            this.Id = id;
            this.ApplyChange(new NeuronCreated(id, tag, layer.Id, authorId));
        }

        /// <summary>
        /// TODO: [Must] Move to another microservice and should be part of security.
        /// </summary>
        /// <param name="layer">Layer neuron Id to be used in the Write process. Specify Guid.Empty to use base layer.</param>
        /// <param name="author">User matched with Subject Id from User database.</param>
        /// <returns></returns>
        internal static void ValidateAuthorAccess(Guid neuronCreatorId, string neuronTag, Neuron layer, Author author)
        {
            AssertionConcern.AssertArgumentValid(g => g != Guid.Empty, neuronCreatorId, Messages.Exception.IdEmpty, nameof(neuronCreatorId));
            AssertionConcern.AssertArgumentNotNull(layer, nameof(layer));
            AssertionConcern.AssertArgumentNotNull(author, nameof(author));
            // TODO: Add TDD tests
            var permit = author.Permits.SingleOrDefault(
                    l => l.LayerNeuronId == layer.Id && l.WriteLevel > 0
                );
            AssertionConcern.AssertStateTrue(permit != null, string.Format(Messages.Exception.UnauthorizedLayerWriteTemplate, layer.Tag));
            AssertionConcern.AssertStateTrue(
                permit.WriteLevel == 2 || neuronCreatorId == author.Neuron.Id, 
                string.Format(Messages.Exception.UnauthorizedNeuronWriteTemplate, neuronTag)
                );
        }

        public bool Active { get; private set; }
        public string Tag { get; private set; }
        public Guid LayerId { get; private set; }
        public Guid CreatorId { get; private set; }

        private void Apply(NeuronCreated e)
        {
            this.Active = true;
            this.Tag = e.Tag;
            this.LayerId = e.LayerId;
            this.CreatorId = e.AuthorId;
        }

        private void Apply(NeuronTagChanged e)
        {
            this.Tag = e.Tag;
        }

        private void Apply(NeuronDeactivated e)
        {
            this.Active = false;
        }

        public void ChangeTag(string value, Neuron layer, Author author)
        {
            AssertionConcern.AssertArgumentNotNull(value, nameof(value));
            AssertionConcern.AssertStateTrue(this.Active, Messages.Exception.NeuronInactive);
            // TODO: Add TDD test
            AssertionConcern.AssertArgumentValid(
                p => p.Id == this.LayerId,
                layer,
                string.Format(Messages.Exception.InvalidNeuronSpecified, layer.Id, this.LayerId),
                nameof(layer)
                );

            // TODO: Add TDD test
            Neuron.ValidateAuthorAccess(this.CreatorId, this.Tag, layer, author);

            if (value != this.Tag)
                base.ApplyChange(new NeuronTagChanged(this.Id, value, author.Neuron.Id));
        }

        public void Deactivate(Neuron layer, Author author)
        {
            AssertionConcern.AssertStateTrue(this.Active, Messages.Exception.NeuronInactive);
            // TODO: Add TDD test
            AssertionConcern.AssertArgumentValid(
                p => p.Id == this.LayerId,
                layer,
                string.Format(Messages.Exception.InvalidNeuronSpecified, layer.Id, this.LayerId),
                nameof(layer)
                );
            // TODO: Add TDD test
            Neuron.ValidateAuthorAccess(this.CreatorId, this.Tag, layer, author);

            this.ApplyChange(new NeuronDeactivated(this.Id, author.Neuron.Id));
        }
    }
}
