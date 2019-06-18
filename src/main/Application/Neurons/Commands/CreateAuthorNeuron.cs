using CQRSlite.Commands;
using Newtonsoft.Json;
using org.neurul.Common.Domain.Model;
using System;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class CreateAuthorNeuron : ICommand
    {
        public CreateAuthorNeuron(string avatarId, Guid id, string tag)
        {
            AssertionConcern.AssertArgumentNotNull(avatarId, nameof(avatarId));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
            AssertionConcern.AssertArgumentNotNull(tag, nameof(tag));

            this.AvatarId = avatarId;
            this.Id = id;            
            this.Tag = tag;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }
        
        public string Tag { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
