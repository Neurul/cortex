using CQRSlite.Commands;
using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class CreateTerminal : ICommand
    {
        public CreateTerminal(string avatarId, Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength, Guid subjectId)
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
                subjectId,
                Messages.Exception.InvalidId,
                nameof(subjectId)
                );

            this.AvatarId = avatarId;
            this.Id = id;
            this.PresynapticNeuronId = presynapticNeuronId;
            this.PostsynapticNeuronId = postsynapticNeuronId;
            this.Effect = effect;
            this.Strength = strength;
            this.SubjectId = subjectId;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }

        public Guid PresynapticNeuronId { get; private set; }

        public Guid PostsynapticNeuronId { get; private set; }

        public NeurotransmitterEffect Effect { get; private set; }

        public float Strength { get; private set; }

        public Guid SubjectId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
