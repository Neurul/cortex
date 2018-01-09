using CQRSlite.Events;
using System;
using System.Collections.Generic;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class NeuronCreated : IEvent
    {
        public readonly string Data;
        
        public NeuronCreated(Guid id, string data)
        {
            this.Id = id;
            this.Data = data;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
