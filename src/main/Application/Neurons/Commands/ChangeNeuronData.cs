using CQRSlite.Commands;
using System;
using System.Collections.Generic;
using org.neurul.Cortex.Domain.Model.Neurons;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class ChangeNeuronData : ICommand
    {
        public readonly string NewData;

        public ChangeNeuronData(Guid id, string newData, int originalVersion)
        {
            this.Id = id;
            this.NewData = newData;
            this.ExpectedVersion = originalVersion;
        }

        public Guid Id { get; private set; }

        public int ExpectedVersion { get; set; }
    }
}
