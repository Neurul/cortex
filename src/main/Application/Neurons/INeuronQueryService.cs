using org.neurul.Cortex.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.Neurons
{
    public interface INeuronQueryService
    {
        Task<NeuronData> GetNeuronById(Guid id, CancellationToken token = default(CancellationToken));
    }
}
