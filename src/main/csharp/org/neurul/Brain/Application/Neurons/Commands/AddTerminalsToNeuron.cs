using CQRSlite.Commands;
using org.neurul.Brain.Domain.Model.Neurons;
using System;
using System.Collections.Generic;

namespace org.neurul.Brain.Application.Neurons.Commands
{
    public class AddTerminalsToNeuron : ICommand
    {
        public readonly IEnumerable<Terminal> Terminals;

        public AddTerminalsToNeuron(Guid id, IEnumerable<Terminal> terminals, int originalVersion)
        {
            this.Id = id;
            this.Terminals = terminals;
            this.ExpectedVersion = originalVersion;
        }

        public Guid Id { get; private set; }

        public int ExpectedVersion { get; set; }
    }
}
