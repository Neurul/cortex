using CQRSlite.Commands;
using neurUL.Cortex.Application.Neurons.Commands;
using neurUL.Cortex.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace neurUL.Cortex.Port.Adapter.In.InProcess
{
    public class TerminalAdapter : ITerminalAdapter
    {
        private readonly ICommandSender commandSender;

        public TerminalAdapter(ICommandSender commandSender)
        {
            this.commandSender = commandSender;
        }

        public async Task CreateTerminal(Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength, Guid authorId)
        {
            await this.commandSender.Send(
                new CreateTerminal(
                    id,
                    presynapticNeuronId,
                    postsynapticNeuronId,
                    effect,
                    strength,
                    authorId
                    )
                );
        }

        public async Task DeactivateTerminal(Guid id, Guid authorId, int expectedVersion)
        {
            await this.commandSender.Send(
                new DeactivateTerminal(
                    id,
                    authorId,
                    expectedVersion
                    )
                );
        }
    }
}
