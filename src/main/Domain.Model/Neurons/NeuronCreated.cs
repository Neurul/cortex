using Newtonsoft.Json;
using org.neurul.Common.Events;
using System;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class NeuronCreated : IAuthoredEvent
    {
        public readonly string Tag;

        public NeuronCreated(Guid id, string tag, Guid authorId)
        {
            this.Id = id;
            this.Tag = tag;
            this.AuthorId = authorId;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        [JsonProperty(PropertyName = "Timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        public Guid AuthorId { get; set; }
    }
}
