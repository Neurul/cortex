using CQRSlite.Events;
using Newtonsoft.Json;
using System;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class NeuronDeactivated : IEvent
    {
        public NeuronDeactivated(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        [JsonProperty(PropertyName = "Timestamp")]
        public DateTimeOffset TimeStamp { get; set; }
    }
}
