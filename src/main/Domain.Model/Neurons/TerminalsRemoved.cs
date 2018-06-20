using org.neurul.Common.Events;
using System;
using System.Collections.Generic;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class TerminalsRemoved : IAuthoredEvent
    {
        public readonly IEnumerable<Terminal> Terminals;

        public TerminalsRemoved(Guid id, IEnumerable<Terminal> terminals, string authorId)
        {
            this.Id = id;
            this.Terminals = terminals;
            this.AuthorId = authorId;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string AuthorId { get; set; }
    }
}
