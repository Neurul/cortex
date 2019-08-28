using CQRSlite.Commands;
using Newtonsoft.Json;
using org.neurul.Common.Domain.Model;
using System;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class CreateNeuron : ICommand
    {
        public CreateNeuron(string avatarId, Guid id, string tag, Guid layerId, Guid subjectId)
        {
            AssertionConcern.AssertArgumentNotNull(avatarId, nameof(avatarId));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
            AssertionConcern.AssertArgumentNotNull(tag, nameof(tag));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                subjectId,
                Messages.Exception.InvalidId,
                nameof(subjectId)
                );

            this.AvatarId = avatarId;
            this.Id = id;            
            this.Tag = tag;
            this.LayerId = layerId;
            this.SubjectId = subjectId;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }
        
        public string Tag { get; private set; }

        public Guid LayerId { get; private set; }

        public Guid SubjectId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
