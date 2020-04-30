using neurUL.Common.Domain.Model;
using neurUL.Cortex.Common;
using System;

namespace neurUL.Cortex.Domain.Model.Neurons
{
    public class Terminal : AssertiveAggregateRoot
    {
        private const string DeactivatedExceptionMessage = "Already deactivated.";

        public Terminal() { }

        public Terminal(Guid id, Neuron presynapticNeuron, Neuron postsynapticNeuron, NeurotransmitterEffect effect, float strength)
        {
            AssertionConcern.AssertArgumentValid(i => i != Guid.Empty, id, Messages.Exception.IdEmpty, nameof(id));
            AssertionConcern.AssertArgumentNotNull(presynapticNeuron, nameof(presynapticNeuron));
            AssertionConcern.AssertArgumentValid(n => n.Active, presynapticNeuron, Messages.Exception.NeuronInactive, nameof(presynapticNeuron));
            AssertionConcern.AssertArgumentNotNull(postsynapticNeuron, nameof(postsynapticNeuron));
            AssertionConcern.AssertArgumentValid(n => n.Active, postsynapticNeuron, Messages.Exception.NeuronInactive, nameof(postsynapticNeuron));
            AssertionConcern.AssertArgumentValid(e => e != NeurotransmitterEffect.NotSet, effect, Messages.Exception.ValidEffect, nameof(effect));
            AssertionConcern.AssertArgumentValid(s => s > 0 && s <= 1, strength, Messages.Exception.StrengthInvalid, nameof(strength));
            AssertionConcern.AssertArgumentValid(g => g != presynapticNeuron.Id, postsynapticNeuron.Id, Messages.Exception.PostCannotBeTheSameAsPre, nameof(postsynapticNeuron));
            AssertionConcern.AssertArgumentValid(g => g != presynapticNeuron.Id, id, Messages.Exception.InvalidTerminalIdCreation, nameof(id));
            AssertionConcern.AssertArgumentValid(g => g != postsynapticNeuron.Id, id, Messages.Exception.InvalidTerminalIdCreation, nameof(id));

            this.Id = id;
            this.ApplyChange(new TerminalCreated(id, presynapticNeuron.Id, postsynapticNeuron.Id, effect, strength));
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

        public void Deactivate()
        {
            AssertionConcern.AssertStateTrue(this.Active, Messages.Exception.TerminalInactive);

            this.ApplyChange(new TerminalDeactivated(this.Id));
        }
    }
}
