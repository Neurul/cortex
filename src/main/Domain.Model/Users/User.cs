using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;

namespace org.neurul.Cortex.Domain.Model.Users
{
    /// <summary>
    /// Represents User Neurons. TODO: Should later be persisted as User Neurons, instead of as records in User databases.
    /// </summary>
    public class User
    {
        /// <summary>
        /// NeuronId of User Neuron in Avatar.
        /// </summary>
        [PrimaryKey]
        public Guid NeuronId { get; set; }
        
        /// <summary>
        /// Subject Id of this User in Identity Access server. This property should be stored as a separate Neuron in a secure layer when it is converted.
        /// </summary>
        public Guid SubjectId { get; set; }
    }
}
