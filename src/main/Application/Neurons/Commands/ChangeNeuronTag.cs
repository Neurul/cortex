using CQRSlite.Commands;
using System;
using System.Collections.Generic;
using org.neurul.Cortex.Domain.Model.Neurons;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class ChangeNeuronTag : ICommand
    {
        public readonly string NewTag;

        public ChangeNeuronTag(string avatarId, Guid id, string newTag, string authorId, int originalVersion)
        {
            this.AvatarId = avatarId;
            this.Id = id;            
            this.NewTag = newTag;
            this.AuthorId = authorId;
            this.ExpectedVersion = originalVersion;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }

        public string AuthorId { get; set; }

        public int ExpectedVersion { get; set; }
    }
}
