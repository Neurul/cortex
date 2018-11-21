using CQRSlite.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using org.neurul.Cortex.Domain.Model.Neurons;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class CreateNeuron : ICommand
    {
        public CreateNeuron()
        {
        }

        [JsonConstructor]
        public CreateNeuron(string avatarId, Guid id, string tag, string authorId)
        {
            this.AvatarId = avatarId;
            this.Id = id;            
            this.Tag = tag;
            this.AuthorId = authorId;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }
        
        public string Tag { get; private set; }

        public string AuthorId { get; set; }

        public int ExpectedVersion { get; set; }
    }
}
