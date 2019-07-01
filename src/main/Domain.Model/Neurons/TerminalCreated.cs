using Newtonsoft.Json;
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

        public TerminalCreated(Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength, Guid authorId)
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

        [JsonProperty(PropertyName = "Timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        public Guid AuthorId { get; set; }
    }
}
