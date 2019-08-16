using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Application.IdentityAccess
{
    public class IdentityPermissionData
    {
        public long Id { get; set; }

        public string SubjectId { get; set; }

        public string Avatar { get; set; }

        public Guid NeuronId { get; set; }

        public IEnumerable<LayerAccessData> AccessibleLayers { get; set; }
    }
}
