using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Domain.Model.IdentityAccess
{
    public interface IIdentityPermissionRepository
    {
        Task<IdentityPermission> GetBySubjectIdAvatar(string subjectId, string avatar);
    }
}
