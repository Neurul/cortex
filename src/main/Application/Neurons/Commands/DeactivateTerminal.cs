using CQRSlite.Commands;
using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Collections.Generic;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class DeactivateTerminal : ICommand
    {
        public DeactivateTerminal(string avatarId, Guid id, string authorId, int expectedVersion)
        {
            AssertionConcern.AssertArgumentNotNull(avatarId, nameof(avatarId));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
            Guid.TryParse(authorId, out Guid gAuthorId);
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                gAuthorId,
                Messages.Exception.InvalidId,
                nameof(authorId)
                );
            AssertionConcern.AssertArgumentValid(
                i => i >= 1,
                expectedVersion,
                Messages.Exception.InvalidExpectedVersion,
                nameof(expectedVersion)
                );

            this.AvatarId = avatarId;
            this.Id = id;
            this.AuthorId = authorId;
            this.ExpectedVersion = expectedVersion;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }

        public string AuthorId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
