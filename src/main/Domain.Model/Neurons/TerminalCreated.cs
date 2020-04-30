using CQRSlite.Events;
using Newtonsoft.Json;
using neurUL.Cortex.Common;
using System;

namespace neurUL.Cortex.Domain.Model.Neurons
{
    public class TerminalCreated : IEvent
    {
        public readonly Guid PresynapticNeuronId;
        public readonly Guid PostsynapticNeuronId;
        public readonly NeurotransmitterEffect Effect;
        public readonly float Strength;

        public TerminalCreated(Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength)
        {
            this.Id = id;
            this.PresynapticNeuronId = presynapticNeuronId;
            this.PostsynapticNeuronId = postsynapticNeuronId;
            this.Effect = effect;
            this.Strength = strength;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        [JsonProperty(PropertyName = "Timestamp")]
        public DateTimeOffset TimeStamp { get; set; }
    }
}
