using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Application.IdentityAccess
{
    public class LayerAccessData
    {
        public string NeuronId { get; set; }

        public bool CanWrite { get; set; }

        public bool CanRead { get; set; }
    }
}
