using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.IdentityAccess
{
    public interface IIdentityPermissionApplicationService
    {
        Task<IdentityPermissionData> GetBySubjectIdAvatar(string subjectId, string avatar);
    }
}
