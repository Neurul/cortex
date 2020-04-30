using CQRSlite.Domain;
using neurUL.Common.Domain.Model;
using System;
using System.Linq;

namespace neurUL.Cortex.Domain.Model.Neurons
{
    /// <summary>
    /// Represents a Neuron.
    /// </summary>
    public class Neuron : AssertiveAggregateRoot
    {
        private Neuron() { }

        /// <summary>
        /// Constructs a Neuron.
        /// </summary>
        /// <param name="id"></param>
        public Neuron(Guid id)
        {
            AssertionConcern.AssertArgumentValid(i => i != Guid.Empty, id, Messages.Exception.IdEmpty, nameof(id));

            this.Id = id;
            this.ApplyChange(new NeuronCreated(id));
        }

        public bool Active { get; private set; }

        private void Apply(NeuronCreated e)
        {
            this.Active = true;
        }

        private void Apply(NeuronDeactivated e)
        {
            this.Active = false;
        }

        public void Deactivate()
        {
            AssertionConcern.AssertStateTrue(this.Active, Messages.Exception.NeuronInactive);

            this.ApplyChange(new NeuronDeactivated(this.Id));
        }
    }
}
