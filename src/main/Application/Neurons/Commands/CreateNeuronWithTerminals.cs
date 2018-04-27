using CQRSlite.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using org.neurul.Cortex.Domain.Model.Neurons;

namespace org.neurul.Cortex.Application.Neurons.Commands
{
    public class CreateNeuronWithTerminals : ICommand
    {
        public CreateNeuronWithTerminals()
        {
        }

        [JsonConstructor]
        public CreateNeuronWithTerminals(string avatarId, Guid id, string data, IEnumerable<Terminal> terminals)
        {
            this.AvatarId = avatarId;
            this.Id = id;            
            this.Data = data;
            this.Terminals = terminals;
        }

        public string AvatarId { get; private set; }

        public Guid Id { get; private set; }
        
        public string Data { get; private set; }

        public IEnumerable<Terminal> Terminals { get; private set; }
        
        public int ExpectedVersion { get; set; }
    }
}
