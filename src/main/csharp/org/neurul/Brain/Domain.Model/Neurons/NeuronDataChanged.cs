using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Brain.Domain.Model.Neurons
{
    public class NeuronDataChanged : IEvent
    {
        public readonly string Data;

        public NeuronDataChanged(Guid id, string data)
        {
            this.Id = id;
            this.Data = data;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
