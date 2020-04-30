using CQRSlite.Commands;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Common;
using neurUL.Cortex.Domain.Model.Neurons;
using System;

namespace neurUL.Cortex.Application.Neurons.Commands
{
    public class CreateTerminal : ICommand
    {
        public CreateTerminal(Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength, Guid authorId)
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                presynapticNeuronId,
                Messages.Exception.InvalidId,
                nameof(presynapticNeuronId)
                );
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                postsynapticNeuronId,
                Messages.Exception.InvalidId,
                nameof(postsynapticNeuronId)
                );
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                authorId,
                Messages.Exception.InvalidId,
                nameof(authorId)
                );

            this.Id = id;
            this.PresynapticNeuronId = presynapticNeuronId;
            this.PostsynapticNeuronId = postsynapticNeuronId;
            this.Effect = effect;
            this.Strength = strength;
            this.AuthorId = authorId;
        }

        public Guid Id { get; private set; }

        public Guid PresynapticNeuronId { get; private set; }

        public Guid PostsynapticNeuronId { get; private set; }

        public NeurotransmitterEffect Effect { get; private set; }

        public float Strength { get; private set; }

        public Guid AuthorId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
