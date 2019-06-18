using CQRSlite.Domain;
using Newtonsoft.Json;
using org.neurul.Common.Domain.Model;
using System;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class Terminal : AggregateRoot
    {
        private const string DeactivatedExceptionMessage = "Already deactivated.";

        public Terminal() { }

        public Terminal(Guid id, Neuron presynapticNeuron, Neuron postsynapticNeuron, NeurotransmitterEffect effect, float strength, Neuron author)
        {
            AssertionConcern.AssertArgumentNotEquals(id, Guid.Empty, Messages.Exception.IdEmpty);
            AssertionConcern.AssertArgumentNotNull(presynapticNeuron, nameof(presynapticNeuron));
            AssertionConcern.AssertArgumentValid(n => n.Active, presynapticNeuron, Messages.Exception.NeuronInactive, nameof(presynapticNeuron));
            AssertionConcern.AssertArgumentNotNull(postsynapticNeuron, nameof(postsynapticNeuron));
            AssertionConcern.AssertArgumentValid(n => n.Active, postsynapticNeuron, Messages.Exception.NeuronInactive, nameof(postsynapticNeuron));
            AssertionConcern.AssertArgumentValid(e => e != NeurotransmitterEffect.NotSet, effect, Messages.Exception.ValidEffect, nameof(effect));
            AssertionConcern.AssertArgumentValid(s => s > 0 && s <= 1, strength, Messages.Exception.StrengthInvalid, nameof(strength));
            AssertionConcern.AssertArgumentNotNull(author, nameof(author));
            AssertionConcern.AssertArgumentValid(n => n.Active, author, Messages.Exception.NeuronInactive, nameof(author));
            AssertionConcern.AssertArgumentValid(g => g != presynapticNeuron.Id, postsynapticNeuron.Id, Messages.Exception.PostCannotBeTheSameAsPre, nameof(postsynapticNeuron));
            AssertionConcern.AssertArgumentValid(g => g != presynapticNeuron.Id, id, Messages.Exception.InvalidTerminalIdCreation, nameof(id));
            AssertionConcern.AssertArgumentValid(g => g != postsynapticNeuron.Id, id, Messages.Exception.InvalidTerminalIdCreation, nameof(id));
            AssertionConcern.AssertArgumentValid(g => g != author.Id, id, Messages.Exception.InvalidTerminalIdCreation, nameof(id));

            this.Id = id;
            this.ApplyChange(new TerminalCreated(id, presynapticNeuron.Id, postsynapticNeuron.Id, effect, strength, author.Id.ToString()));
        }

        public bool Active { get; private set; }
        public Guid PresynapticNeuronId { get; private set; }
        public Guid PostsynapticNeuronId { get; private set; }
        public NeurotransmitterEffect Effect { get; private set; }
        public float Strength { get; private set; }

        private void Apply(TerminalCreated e)
        {
            this.Active = true;
            this.PresynapticNeuronId = e.PresynapticNeuronId;
            this.PostsynapticNeuronId = e.PostsynapticNeuronId;
            this.Effect = e.Effect;
            this.Strength = e.Strength;
        }

        private void Apply(TerminalDeactivated e)
        {
            this.Active = false;
        }

        public void Deactivate(Neuron author)
        {
            AssertionConcern.AssertArgumentNotNull(author, nameof(author));
            AssertionConcern.AssertArgumentValid(n => n.Active, author, Messages.Exception.NeuronInactive, nameof(author));
            AssertionConcern.AssertStateTrue(this.Active, Messages.Exception.NeuronInactive);

            this.ApplyChange(new TerminalDeactivated(this.Id, author.Id.ToString()));
        }
    }
}
