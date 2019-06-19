using org.neurul.Common.Events;
using System;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class NeuronDeactivated : IAuthoredEvent
    {
        public NeuronDeactivated(Guid id, Guid authorId)
        {
            this.Id = id;
            this.AuthorId = authorId;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public Guid AuthorId { get; set; }
    }
}
