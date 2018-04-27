using CQRSlite.Commands;
using System;
using System.Collections.Generic;
using org.neurul.Cortex.Domain.Model.Neurons;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class DeactivateNeuron : ICommand
    {
        public DeactivateNeuron(string avatarId, Guid id, int originalVersion)
        {
            this.AvatarId = avatarId;
            this.Id = id;
            
            this.ExpectedVersion = originalVersion;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }
        
        public int ExpectedVersion { get; set; }
    }
}
