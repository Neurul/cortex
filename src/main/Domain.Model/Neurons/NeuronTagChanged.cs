using org.neurul.Common.Events;
using System;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class NeuronTagChanged : IAuthoredEvent
    {
        public readonly string Tag;

        public NeuronTagChanged(Guid id, string tag, string authorId)
        {
            this.Id = id;
            this.Tag = tag;
            this.AuthorId = authorId;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string AuthorId { get; set; }
    }
}
