using neurUL.Cortex.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace neurUL.Cortex.Port.Adapter.In.InProcess
{
    public interface ITerminalAdapter
    {
        Task CreateTerminal(Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength, Guid authorId);

        Task DeactivateTerminal(Guid id, Guid authorId, int expectedVersion);
    }
}
