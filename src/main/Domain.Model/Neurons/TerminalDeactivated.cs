using Newtonsoft.Json;
using org.neurul.Common.Events;
using System;
using System.Collections.Generic;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public class TerminalDeactivated : IAuthoredEvent
    {
        public TerminalDeactivated(Guid id, Guid authorId)
        {
            this.Id = id;
            this.AuthorId = authorId;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        [JsonProperty(PropertyName = "Timestamp")]
        public DateTimeOffset TimeStamp { get; set; }

        public Guid AuthorId { get; set; }
    }
}
