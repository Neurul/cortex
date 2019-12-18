using CQRSlite.Commands;
using org.neurul.Common.Domain.Model;
using System;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class CreateNeuron : ICommand
    {
        public CreateNeuron(string avatarId, Guid id, Guid authorId)
        {
            AssertionConcern.AssertArgumentNotNull(avatarId, nameof(avatarId));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                authorId,
                Messages.Exception.InvalidId,
                nameof(authorId)
                );

            this.AvatarId = avatarId;
            this.Id = id;            
            this.AuthorId = authorId;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }
        
        public Guid AuthorId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
