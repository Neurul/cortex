using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Domain.Model.IdentityAccess;
using org.neurul.Cortex.Port.Adapter.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Port.Adapter.IO.Persistence.IdentityAccess
{
    public class IdentityPermissionRepository : IIdentityPermissionRepository
    {
        private SQLiteAsyncConnection connection;

        public IdentityPermissionRepository()
        {
            string databasePath = Environment.GetEnvironmentVariable(EnvironmentVariableKeys.IdentityAccessDatabasePath);
            AssertionConcern.AssertPathValid(databasePath, EnvironmentVariableKeys.IdentityAccessDatabasePath);

            this.connection = new SQLiteAsyncConnection(databasePath);
            this.connection.CreateTableAsync<IdentityPermission>();
            //sample data creator
            //this.connection.InsertAsync(new IdentityPermission()
            //{
            //    NeuronId = Guid.NewGuid(),
            //    Avatar = "test",
            //    SubjectId = Guid.NewGuid().ToString(),
            //    AccessibleLayers = new LayerAccess[] {
            //        new LayerAccess()
            //        {
            //            NeuronId = Guid.NewGuid().ToString(),
            //            CanRead = true,
            //            CanWrite = true
            //        }
            //    }
            //});
        }

        public async Task<IdentityPermission> GetBySubjectIdAvatar(string subjectId, string avatar)
        {
            var results = this.connection.Table<IdentityPermission>().Where(
                e => e.SubjectId == subjectId && e.Avatar == avatar
                );
            return (await results.ToListAsync()).SingleOrDefault();
        }
    }
}
