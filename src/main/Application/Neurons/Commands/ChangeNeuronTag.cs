﻿using CQRSlite.Commands;
using org.neurul.Common.Domain.Model;
using System;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class ChangeNeuronTag : ICommand
    {
        public ChangeNeuronTag(string avatarId, Guid id, string newTag, Guid subjectId, int expectedVersion)
        {
            AssertionConcern.AssertArgumentNotNull(avatarId, nameof(avatarId));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );
            AssertionConcern.AssertArgumentNotNull(newTag, nameof(newTag));
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                subjectId,
                Messages.Exception.InvalidId,
                nameof(subjectId)
                );
            AssertionConcern.AssertArgumentValid(
                i => i >= 1,
                expectedVersion,
                Messages.Exception.InvalidExpectedVersion,
                nameof(expectedVersion)
                );

            this.AvatarId = avatarId;
            this.Id = id;            
            this.NewTag = newTag;
            this.SubjectId = subjectId;
            this.ExpectedVersion = expectedVersion;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }

        public string NewTag { get; private set; }

        public Guid SubjectId { get; private set; }

        public int ExpectedVersion { get; private set; }
    }
}
