using CQRSlite.Commands;
using neurUL.Cortex.Application.Neurons.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace neurUL.Cortex.Port.Adapter.In.InProcess
{
    public class NeuronAdapter : INeuronAdapter
    {
        private readonly ICommandSender commandSender;

        public NeuronAdapter(ICommandSender commandSender)
        {
            this.commandSender = commandSender;
        }

        public async Task CreateNeuron(Guid id, Guid authorId)
        {
            await this.commandSender.Send(new CreateNeuron(id, authorId));
        }

        public async Task DeactivateNeuron(Guid id, Guid authorId, int expectedVersion)
        {
            await this.commandSender.Send(new DeactivateNeuron(id, authorId, expectedVersion));
        }
    }
}
