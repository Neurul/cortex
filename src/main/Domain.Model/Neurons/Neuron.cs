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
        private const string DeactivatedExceptionMessage = "Already deactivated.";

        private Neuron() { }

        public Neuron(Guid id, string tag, string authorId)
        {
            this.Id = id;
            this.ApplyChange(new NeuronCreated(id, tag, authorId));
        }

        public bool Active { get; private set; }
        public string Tag { get; private set; }

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
            this.Active = true;
            this.Tag = e.Tag;
        }

        private void Apply(NeuronTagChanged e)
        {
            this.Tag = e.Tag;
        }

        private void Apply(NeuronDeactivated e)
        {
            this.Active = false;
        }

        private void Apply(TerminalsAdded e)
        {
            this.terminals.AddRange(e.Terminals);
        }

        private void Apply(TerminalsRemoved e)
        {
            e.TargetIds.ToList().ForEach(t => this.terminals.RemoveAll(te => te.TargetId.ToString() == t));
        }

        public void ChangeTag(string value, string authorId)
        {
            this.AssertActive();

            if (value != this.Tag)
                base.ApplyChange(new NeuronTagChanged(this.Id, value, authorId));
        }

        public void Deactivate(string authorId)
        {
            this.AssertActive();

            this.ApplyChange(new NeuronDeactivated(this.Id, authorId));
        }

        public void RemoveTerminals(IEnumerable<string> targetIds, string authorId)
        {
            this.AssertActive();

            if (targetIds == null)
                throw new ArgumentNullException(nameof(targetIds));
            if (!targetIds.Any())
                throw new ArgumentException("Specified Terminal list cannot be empty.", nameof(targetIds));
            string nft = string.Empty;
            if (targetIds.Any(t => { nft = t; return !this.terminals.Any(te => te.TargetId.ToString() == t); }))
                throw new ArgumentException($"Specified Terminal '{nft.ToString()}' was not found.", nameof(targetIds));

            base.ApplyChange(new TerminalsRemoved(this.Id, targetIds, authorId));
        }

        private void AssertActive()
        {
            if (!this.Active)
                throw new InvalidOperationException(Neuron.DeactivatedExceptionMessage);
        }
    }
}
