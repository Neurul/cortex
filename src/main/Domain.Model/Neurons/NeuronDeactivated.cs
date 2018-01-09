using CQRSlite.Events;
using System;
using System.Collections.Generic;

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

        public DateTimeOffset TimeStamp { get; set; }
    }
}
