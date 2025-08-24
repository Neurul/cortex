using neurUL.Common.CqrsLite;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Common;
using System;

namespace neurUL.Cortex.Domain.Model.Neurons
{
    public class Terminal : AssertiveAggregateRoot
    {
        private const string DeactivatedExceptionMessage = "Already deactivated.";

        public Terminal() { }

        public Terminal(Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength)
        {
            AssertionConcern.AssertArgumentValid(i => i != Guid.Empty, id, Messages.Exception.IdEmpty, nameof(id));
            AssertionConcern.AssertArgumentValid(i => i != Guid.Empty, presynapticNeuronId, Messages.Exception.IdEmpty, nameof(presynapticNeuronId));
            AssertionConcern.AssertArgumentValid(i => i != Guid.Empty, postsynapticNeuronId, Messages.Exception.IdEmpty, nameof(postsynapticNeuronId));
            AssertionConcern.AssertArgumentValid(e => e != NeurotransmitterEffect.NotSet, effect, Messages.Exception.ValidEffect, nameof(effect));
            AssertionConcern.AssertArgumentValid(s => s > 0 && s <= 1, strength, Messages.Exception.StrengthInvalid, nameof(strength));
            AssertionConcern.AssertArgumentValid(g => g != presynapticNeuronId, postsynapticNeuronId, Messages.Exception.PostCannotBeTheSameAsPre, nameof(postsynapticNeuronId));
            AssertionConcern.AssertArgumentValid(g => g != presynapticNeuronId, id, Messages.Exception.InvalidTerminalIdCreation, nameof(id));
            AssertionConcern.AssertArgumentValid(g => g != postsynapticNeuronId, id, Messages.Exception.InvalidTerminalIdCreation, nameof(id));

            this.Id = id;
            this.ApplyChange(new TerminalCreated(id, presynapticNeuronId, postsynapticNeuronId, effect, strength));
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
