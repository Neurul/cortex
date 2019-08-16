using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Domain.Model.IdentityAccess
{
    public class IdentityPermission
    {
        [AutoIncrement]
        [PrimaryKey]
        public long Id { get; set; }

        /// <summary>
        /// Subject Id of this Identity in Identity Access server. This property should be stored as a separate Neuron in a secure layer when it is converted.
        /// </summary>
        public string SubjectId { get; set; }

        /// <summary>
        /// Avatar this Permission pertains to.
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// Author Neuron in Avatar mapped to this Identity. This will be the NeuronId of the grandmother cell of this Avatar User.
        /// </summary>
        public Guid NeuronId { get; set; }

        /// <summary>
        /// Layers in Avatar this Identity can access. This property should be stored as a separate Neuron in a secure layer when it is converted.
        /// </summary>
        [Ignore]
        public IEnumerable<LayerAccess> AccessibleLayers { get; set; }

        public string AccessibleLayersBlob
        {
            get
            {
                return JsonConvert.SerializeObject(this.AccessibleLayers);
            }
            set
            {
                this.AccessibleLayers = JsonConvert.DeserializeObject<IEnumerable<LayerAccess>>(value);
            }
        }
    }
}
