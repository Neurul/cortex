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
        public CreateNeuron(Guid id, string data)
        {
            this.Id = id;
            this.Data = data;
        }

        public Guid Id { get; private set; }

        public string Data { get; private set; }
        
        public int ExpectedVersion { get; set; }
    }
}
