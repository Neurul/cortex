using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public struct Terminal
    {
        public Terminal(Terminal original) : this(original.TargetId)
        {
        }

        [JsonConstructor]
        public Terminal(Guid targetId)
        { 
            this.TargetId = targetId;
        }

        public static readonly Terminal Empty = new Terminal(Guid.Empty);

        public Guid TargetId { get; private set; }
    }
}
