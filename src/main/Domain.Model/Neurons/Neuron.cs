using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class Neuron : AggregateRoot
    {
        private readonly List<Terminal> axon = new List<Terminal>();
        private bool activated;
        private const string DeactivatedExceptionMessage = "Already deactivated.";

        private Neuron() { }

        public Neuron(Guid id) : this(id, string.Empty) { }

        public Neuron(Guid id, string data)
        {
            this.Id = id;
            this.ApplyChange(new NeuronCreated(id, data));
        }

        public string Data { get; private set; }

        public ReadOnlyCollection<Terminal> Axon
        {
            get
            {
                return this.axon.AsReadOnly();
            }
        }
        
        public async Task AddTerminals(ILinkService linkService, params Terminal[] terminals)
        {
            await this.AddTerminals(linkService, (IEnumerable<Terminal>) terminals);
        }

        public async Task AddTerminals(ILinkService linkService, IEnumerable<Terminal> terminals)
        {
            if (terminals == null) throw new ArgumentNullException(nameof(terminals));
            if (linkService == null) throw new ArgumentNullException(nameof(linkService));
            if (!terminals.Any())
                throw new ArgumentException("Specified 'terminals' is empty.", nameof(terminals));
            this.AssertActive();
            foreach (Terminal t in terminals)
                if (!(await linkService.IsValidTarget(t.Target)))
                    throw new ArgumentException($"Specified Terminal Target '{ t.Target.ToString() }' does not exist or has been deactivated.", nameof(terminals));
            Guid target = Guid.Empty;
            if (terminals.Any(t => { target = t.Target; return axon.Any(a => a.Target == t.Target); }))
                throw new ArgumentException($"Specified Terminal Target '{ target.ToString() }' already exists.", nameof(terminals));

            base.ApplyChange(new TerminalsAdded(this.Id, terminals));
        }

        private void Apply(NeuronCreated e)
        {
            this.activated = true;
            this.Data = e.Data;
        }

        private void Apply(NeuronDataChanged e)
        {
            this.Data = e.Data;
        }

        private void Apply(NeuronDeactivated e)
        {
            this.activated = false;
        }

        private void Apply(TerminalsAdded e)
        {
            this.axon.AddRange(e.Terminals);
        }

        private void Apply(TerminalsRemoved e)
        {
            e.Terminals.ToList().ForEach(t => this.axon.Remove(t));
        }

        public void ChangeData(string value)
        {
            this.AssertActive();

            if (value != this.Data)
                base.ApplyChange(new NeuronDataChanged(this.Id, value));
        }

        public void Deactivate()
        {
            this.AssertActive();

            this.ApplyChange(new NeuronDeactivated(this.Id));
        }

        public void RemoveTerminals(params Terminal[] terminals)
        {
            this.RemoveTerminals((IEnumerable<Terminal>)terminals);
        }

        public void RemoveTerminals(IEnumerable<Terminal> terminals)
        {
            this.AssertActive();

            if (terminals == null)
                throw new ArgumentNullException(nameof(terminals));
            if (!terminals.Any())
                throw new ArgumentException("Specified Terminal list cannot be empty.", nameof(terminals));
            Terminal nft = Terminal.Empty;
            if (terminals.Any(t => { nft = t; return !this.axon.Contains(t); }))
                throw new ArgumentException($"Specified Terminal '{nft.Target.ToString()}' was not found.", nameof(terminals));

            base.ApplyChange(new TerminalsRemoved(this.Id, terminals));
        }

        private void AssertActive()
        {
            if (!this.activated)
                throw new InvalidOperationException(Neuron.DeactivatedExceptionMessage);
        }
    }
}
