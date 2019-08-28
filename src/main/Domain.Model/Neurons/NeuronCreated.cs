using Newtonsoft.Json;
using org.neurul.Common.Events;
using System;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class NeuronCreated : IAuthoredEvent
    {
        public NeuronCreated(Guid id, string tag, Guid layerId, Guid authorId)
        {
            this.Id = id;
            this.Tag = tag;
            this.LayerId = layerId;
            this.AuthorId = authorId;
        }

        public Guid Id { get; set; }

        public string Tag { get; set; }

        public Guid LayerId { get; set; }

        public int Version { get; set; }

        [JsonProperty(PropertyName = "Timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        public Guid AuthorId { get; set; }
    }
}
