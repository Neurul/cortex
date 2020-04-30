using CQRSlite.Events;
using Newtonsoft.Json;
using System;

namespace neurUL.Cortex.Domain.Model.Neurons
{
    public class NeuronCreated : IEvent
    {
        public NeuronCreated(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        [JsonProperty(PropertyName = "Timestamp")]
        public DateTimeOffset TimeStamp { get; set; }
    }
}
