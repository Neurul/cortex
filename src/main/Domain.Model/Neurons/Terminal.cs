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
        public Terminal(Terminal original) : this(original.Target)
        {
        }

        [JsonConstructor]
        public Terminal(Guid target)
        { 
            this.Target = target;
        }

        public static readonly Terminal Empty = new Terminal(Guid.Empty);

        public Guid Target { get; private set; }
    }
}
