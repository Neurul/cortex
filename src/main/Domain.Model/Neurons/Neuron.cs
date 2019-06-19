using CQRSlite.Domain;
using org.neurul.Common.Domain.Model;
using System;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class Neuron : AssertiveAggregateRoot
    {
        private Neuron() { }

        public Neuron(Guid id, string tag)
        {
            this.Construct(id, tag, id);
        }

        public Neuron(Guid id, string tag, Neuron author)
        {
            AssertionConcern.AssertArgumentNotNull(author, nameof(author));
            AssertionConcern.AssertArgumentValid(n => n.Active, author, Messages.Exception.NeuronInactive, nameof(author));
            AssertionConcern.AssertArgumentValid(g => g != author.Id, id, Messages.Exception.NonAuthorConstructor, nameof(id));

            this.Construct(id, tag, author.Id);
        }

        private void Construct(Guid id, string tag, Guid authorId)
        {
            AssertionConcern.AssertArgumentNotEquals(id, Guid.Empty, Messages.Exception.IdEmpty);
            AssertionConcern.AssertArgumentNotNull(tag, nameof(tag));

            this.Id = id;
            this.ApplyChange(new NeuronCreated(id, tag, authorId));
        }

        public bool Active { get; private set; }
        public string Tag { get; private set; }

        private void Apply(NeuronCreated e)
        {
            this.Active = true;
            this.Tag = e.Tag;
        }

        private void Apply(NeuronTagChanged e)
        {
            this.Tag = e.Tag;
        }

        private void Apply(NeuronDeactivated e)
        {
            this.Active = false;
        }

        public void ChangeTag(string value, Neuron author)
        {
            AssertionConcern.AssertArgumentNotNull(value, nameof(value));
            AssertionConcern.AssertArgumentNotNull(author, nameof(author));
            AssertionConcern.AssertArgumentValid(n => n.Active, author, Messages.Exception.NeuronInactive, nameof(author));
            AssertionConcern.AssertStateTrue(this.Active, Messages.Exception.NeuronInactive);

            if (value != this.Tag)
                base.ApplyChange(new NeuronTagChanged(this.Id, value, author.Id));
        }

        public void Deactivate(Neuron author)
        {
            AssertionConcern.AssertArgumentNotNull(author, nameof(author));
            AssertionConcern.AssertArgumentValid(n => n.Active, author, Messages.Exception.NeuronInactive, nameof(author));
            AssertionConcern.AssertStateTrue(this.Active, Messages.Exception.NeuronInactive);

            this.ApplyChange(new NeuronDeactivated(this.Id, author.Id));
        }
    }
}
