using neurUL.Cortex.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace neurUL.Cortex.Application.Neurons
{
    public interface INeuronQueryService
    {
        Task<NeuronData> GetNeuronById(Guid id, CancellationToken token = default(CancellationToken));
    }
}
