using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Domain.Model.IdentityAccess
{
    public class LayerAccess
    {
        public string NeuronId { get; set; }

        public bool CanWrite { get; set; }

        public bool CanRead { get; set; }
    }
}
