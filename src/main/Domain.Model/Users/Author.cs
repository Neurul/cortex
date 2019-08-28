using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Domain.Model.Users
{
    public class Author
    {
        public Author(Neuron neuron, User user, IEnumerable<LayerPermit> permits)
        {
            // TODO: Add TDD test
            AssertionConcern.AssertArgumentNotNull(neuron, nameof(neuron));
            AssertionConcern.AssertArgumentValid(n => n.Active, neuron, Messages.Exception.NeuronInactive, nameof(neuron));
            AssertionConcern.AssertArgumentNotNull(user, nameof(user));
            AssertionConcern.AssertArgumentNotNull(permits, nameof(permits));

            this.Neuron = neuron;
            this.User = user;
            this.Permits = permits;
        }

        public Neuron Neuron { get; private set; }

        public User User { get; private set; }

        public IEnumerable<LayerPermit> Permits { get; private set; }
    }
}
