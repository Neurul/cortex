using CQRSlite.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using org.neurul.Brain.Domain.Model.Neurons;

namespace org.neurul.Brain.Application.Neurons.Commands
{
    public class CreateNeuronWithTerminals : ICommand
    {
        public CreateNeuronWithTerminals()
        {
        }

        [JsonConstructor]
        public CreateNeuronWithTerminals(Guid id, string data, IEnumerable<Terminal> terminals)
        {
            this.Id = id;
            this.Data = data;
            this.Terminals = terminals;
        }

        public Guid Id { get; private set; }

        public string Data { get; private set; }

        public IEnumerable<Terminal> Terminals { get; private set; }
        
        public int ExpectedVersion { get; set; }
    }
}
