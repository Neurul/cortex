using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using System;

namespace neurUL.Cortex.Application.Neurons.Commands
{
    public class CreateNeuron : ICommand
    {
        public CreateNeuron(Guid id, Guid authorId)
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

            this.Id = id;            
            this.AuthorId = authorId;
        }

        public Guid Id { get; private set; }
        
        public Guid AuthorId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
