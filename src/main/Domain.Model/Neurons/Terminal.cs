using Newtonsoft.Json;
using System;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public struct Terminal
    {
        public Terminal(Terminal original) : this(original.TargetId, original.Effect, original.Strength)
        {
        }

        [JsonConstructor]
        public Terminal(Guid targetId, NeurotransmitterEffect effect, float strength)
        { 
            this.TargetId = targetId;
            this.Effect = effect;
            this.Strength = strength;
        }

        public static readonly Terminal Empty = new Terminal(Guid.Empty, NeurotransmitterEffect.Excite, 1);

        public Guid TargetId { get; private set; }

        public NeurotransmitterEffect Effect { get; private set; }

        public float Strength { get; private set; }
    }
}
