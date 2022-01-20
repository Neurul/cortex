using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace neurUL.Cortex.Port.Adapter.In.InProcess
{
    public interface INeuronAdapter
    {
        Task CreateNeuron(Guid id, Guid authorId);

        Task DeactivateNeuron(Guid id, Guid authorId, int expectedVersion);
    }
}
