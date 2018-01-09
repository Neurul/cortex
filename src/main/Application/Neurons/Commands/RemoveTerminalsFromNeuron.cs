using CQRSlite.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Collections.Generic;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class RemoveTerminalsFromNeuron : ICommand
    {
        public readonly IEnumerable<Terminal> Terminals;

        public RemoveTerminalsFromNeuron(Guid id, IEnumerable<Terminal> terminals, int originalVersion)
        {
            this.Id = id;
            this.Terminals = terminals;
            this.ExpectedVersion = originalVersion;
        }

        public Guid Id { get; private set; }

        public int ExpectedVersion { get; set; }
    }
}
