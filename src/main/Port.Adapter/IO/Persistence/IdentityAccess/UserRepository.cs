using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Domain.Model.Users;
using org.neurul.Cortex.Port.Adapter.Common;
using SQLite;
using System;
using System.Linq;
using System.Threading.Tasks;
using org.neurul.Cortex.Port.Adapter.Common;

namespace org.neurul.Cortex.Port.Adapter.IO.Persistence.IdentityAccess
{
    public class UserRepository : IUserRepository
    {
        private SQLiteAsyncConnection connection;

        public async Task<User> GetBySubjectId(Guid subjectId)
        {
            var results = this.connection.Table<User>().Where(e => e.SubjectId == subjectId);
            return (await results.ToListAsync()).SingleOrDefault();
        }

        public async Task Initialize(string storeId)
        {
            AssertionConcern.AssertArgumentNotNull(storeId, nameof(storeId));
            AssertionConcern.AssertArgumentNotEmpty(storeId, $"'{nameof(storeId)}' cannot be empty.", nameof(storeId));

            this.connection = await UserRepository.CreateConnection<User>(storeId);

            //sample data creator - call Initialize from CustomBootstrapper to invoke
            //await this.connection.InsertAsync(new User()
            //{
            //    NeuronId = Guid.NewGuid(),
            //    SubjectId = Guid.NewGuid()
            //});
        }

        // TODO: Transfer to NeurUL.Common
        internal static async Task<SQLiteAsyncConnection> CreateConnection<TTable>(string storeId) where TTable : new()
        {
            SQLiteAsyncConnection result = null;
            string databasePath = string.Format(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.UserDatabasePath), storeId);

            if (!databasePath.Contains(":memory:"))
                AssertionConcern.AssertPathValid(databasePath, nameof(databasePath));

            result = new SQLiteAsyncConnection(databasePath);
            await result.CreateTableAsync<TTable>();
            return result;
        }
    }
}
