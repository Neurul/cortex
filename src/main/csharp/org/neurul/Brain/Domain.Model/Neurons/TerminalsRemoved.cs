using CQRSlite.Events;
using System;
using System.Collections.Generic;

namespace org.neurul.Brain.Domain.Model.Neurons
{
    public class TerminalsRemoved : IEvent
    {
        public readonly IEnumerable<Terminal> Terminals;

        public TerminalsRemoved(Guid id, IEnumerable<Terminal> terminals)
        {
            this.Id = id;
            this.Terminals = terminals;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
