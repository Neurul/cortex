using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Domain.Model.Users
{
    public interface ILayerPermitRepository
    {
        Task<IEnumerable<LayerPermit>> GetAllByUserNeuronId(Guid userNeuronId);

        Task Initialize(string storeId);
    }
}
