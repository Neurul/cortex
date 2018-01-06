using CQRSlite.Commands;
using System;
using System.Collections.Generic;
using org.neurul.Brain.Domain.Model.Neurons;

namespace org.neurul.Brain.Application.Neurons.Commands
{
    public class DeactivateNeuron : ICommand
    {
        public DeactivateNeuron(Guid id, int originalVersion)
        {
            this.Id = id;
            this.ExpectedVersion = originalVersion;
        }

        public Guid Id { get; private set; }

        public int ExpectedVersion { get; set; }
    }
}
