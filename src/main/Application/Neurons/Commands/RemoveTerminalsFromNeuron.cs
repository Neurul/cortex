using CQRSlite.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Collections.Generic;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class RemoveTerminalsFromNeuron : ICommand
    {
        public readonly IEnumerable<string> TargetIds;

        public RemoveTerminalsFromNeuron(string avatarId, Guid id, IEnumerable<string> targetIds, string authorId, int originalVersion)
        {
            this.AvatarId = avatarId;
            this.Id = id;
            this.TargetIds = targetIds;
            this.AuthorId = authorId;
            this.ExpectedVersion = originalVersion;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }

        public string AuthorId { get; set; }

        public int ExpectedVersion { get; set; }
    }
}
