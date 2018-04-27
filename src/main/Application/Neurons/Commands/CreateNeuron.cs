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
        public CreateNeuron(string avatarId, Guid id, string data)
        {
            this.AvatarId = avatarId;
            this.Id = id;            
            this.Data = data;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }
        
        public string Data { get; private set; }
        
        public int ExpectedVersion { get; set; }
    }
}
