using CQRSlite.Domain;
using org.neurul.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class Neuron : AggregateRoot
    {
        private readonly List<Terminal> terminals = new List<Terminal>();
        private bool activated;
        private const string DeactivatedExceptionMessage = "Already deactivated.";

        private Neuron() { }

        public Neuron(Guid id, string data, string authorId)
        {
            this.Id = id;
            this.ApplyChange(new NeuronCreated(id, data, authorId));
        }

        public string Data { get; private set; }

        public ReadOnlyCollection<Terminal> Terminals
        {
            get
            {
                return this.terminals.AsReadOnly();
            }
        }
        
        public Task AddTerminals(string authorId, params Terminal[] terminals)
        {
            return this.AddTerminals((IEnumerable<Terminal>) terminals, authorId);
        }

        public Task AddTerminals(IEnumerable<Terminal> terminals, string authorId)
        {
            if (terminals == null) throw new ArgumentNullException(nameof(terminals));
            if (!terminals.Any())
                throw new ArgumentException("Specified 'terminals' is empty.", nameof(terminals));
            // TODO: AssertionConcern.AssertArgumentNotEmpty(authorId, "Specified 'authorId' is empty.", nameof(authorId));
            this.AssertActive();
            Guid target = Guid.Empty;
            if (terminals.Any(t => { target = t.TargetId; return this.terminals.Any(a => a.TargetId == t.TargetId); }))
                throw new ArgumentException($"Specified Terminal Target '{ target.ToString() }' already exists.", nameof(terminals));

            base.ApplyChange(new TerminalsAdded(this.Id, terminals, authorId));

            return Task.CompletedTask;
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
            this.terminals.AddRange(e.Terminals);
        }

        private void Apply(TerminalsRemoved e)
        {
            e.Terminals.ToList().ForEach(t => this.terminals.Remove(t));
        }

        public void ChangeData(string value, string authorId)
        {
            this.AssertActive();

            if (value != this.Data)
                base.ApplyChange(new NeuronDataChanged(this.Id, value, authorId));
        }

        public void Deactivate(string authorId)
        {
            this.AssertActive();

            this.ApplyChange(new NeuronDeactivated(this.Id, authorId));
        }

        public void RemoveTerminals(string authorId, params Terminal[] terminals)
        {
            this.RemoveTerminals((IEnumerable<Terminal>)terminals, authorId);
        }

        public void RemoveTerminals(IEnumerable<Terminal> terminals, string authorId)
        {
            this.AssertActive();

            if (terminals == null)
                throw new ArgumentNullException(nameof(terminals));
            if (!terminals.Any())
                throw new ArgumentException("Specified Terminal list cannot be empty.", nameof(terminals));
            Terminal nft = Terminal.Empty;
            if (terminals.Any(t => { nft = t; return !this.terminals.Contains(t); }))
                throw new ArgumentException($"Specified Terminal '{nft.TargetId.ToString()}' was not found.", nameof(terminals));

            base.ApplyChange(new TerminalsRemoved(this.Id, terminals, authorId));
        }

        private void AssertActive()
        {
            if (!this.activated)
                throw new InvalidOperationException(Neuron.DeactivatedExceptionMessage);
        }
    }
}
