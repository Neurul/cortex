using org.neurul.Common.Events;
using System;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class NeuronDataChanged : IAuthoredEvent
    {
        public readonly string Data;

        public NeuronDataChanged(Guid id, string data, string authorId)
        {
            this.Id = id;
            this.Data = data;
            this.AuthorId = authorId;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string AuthorId { get; set; }
    }
}
