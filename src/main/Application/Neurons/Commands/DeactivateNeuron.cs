using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using System;

namespace neurUL.Cortex.Application.Neurons.Commands
{
    public class DeactivateNeuron : ICommand
    {
        public DeactivateNeuron(Guid id, Guid authorId, int expectedVersion)
        {
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
            AssertionConcern.AssertArgumentValid(
                i => i >= 1,
                expectedVersion,
                Messages.Exception.InvalidExpectedVersion,
                nameof(expectedVersion)
                );

            this.Id = id;
            this.AuthorId = authorId;            
            this.ExpectedVersion = expectedVersion;
        }

        public Guid Id { get; private set; }

        public Guid AuthorId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
