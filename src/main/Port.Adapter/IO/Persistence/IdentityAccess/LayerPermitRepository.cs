using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Domain.Model.Users;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Port.Adapter.IO.Persistence.IdentityAccess
{
    public class LayerPermitRepository : ILayerPermitRepository
    {
        private SQLiteAsyncConnection connection;

        public async Task<IEnumerable<LayerPermit>> GetAllByUserNeuronId(Guid userNeuronId)
        {
            var results = this.connection.Table<LayerPermit>().Where(e => e.UserNeuronId == userNeuronId);
            return (await results.ToArrayAsync());
        }

        public async Task Initialize(string storeId)
        {
            AssertionConcern.AssertArgumentNotNull(storeId, nameof(storeId));
            AssertionConcern.AssertArgumentNotEmpty(storeId, $"'{nameof(storeId)}' cannot be empty.", nameof(storeId));

            this.connection = await UserRepository.CreateConnection<LayerPermit>(storeId);

            //sample data creator - call Initialize from CustomBootstrapper to invoke
            //await this.connection.InsertAsync(new LayerPermit()
            //{
            //    UserNeuronId = Guid.NewGuid(),
            //    LayerNeuronId = Guid.NewGuid(),
            //    CanWrite = true,
            //    CanRead = true
            //});
        }
    }
}
