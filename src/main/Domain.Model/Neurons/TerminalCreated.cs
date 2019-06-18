using org.neurul.Common.Events;
using System;
using System.Collections.Generic;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class TerminalCreated : IAuthoredEvent
    {
        public readonly Guid PresynapticNeuronId;
        public readonly Guid PostsynapticNeuronId;
        public readonly NeurotransmitterEffect Effect;
        public readonly float Strength;

        public TerminalCreated(Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength, string authorId)
        {
            this.Id = id;
            this.PresynapticNeuronId = presynapticNeuronId;
            this.PostsynapticNeuronId = postsynapticNeuronId;
            this.Effect = effect;
            this.Strength = strength;
            this.AuthorId = authorId;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string AuthorId { get; set; }
    }
}
