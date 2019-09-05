using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Domain.Model.Users
{
    public interface ILayerPermitRepository
    {
        // TODO: change to GetAllApplicableByUserNeuronId - will include layers that have a Guid.Empty as UserNeuronId to indicate permits that apply to all users
        Task<IEnumerable<LayerPermit>> GetAllByUserNeuronId(Guid userNeuronId);

        Task Initialize(string storeId);
    }
}
