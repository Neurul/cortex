using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Domain.Model.IdentityAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.IdentityAccess
{
    public class IdentityPermissionApplicationService : IIdentityPermissionApplicationService
    {
        public IdentityPermissionApplicationService(IIdentityPermissionRepository identityPermissionRepository)
        {
            AssertionConcern.AssertArgumentNotNull(identityPermissionRepository, nameof(identityPermissionRepository));
            this.identityPermissionRepository = identityPermissionRepository;
        }

        readonly IIdentityPermissionRepository identityPermissionRepository;

        public async Task<IdentityPermissionData> GetBySubjectIdAvatar(string subjectId, string avatar)
        {
            var ip = await this.identityPermissionRepository.GetBySubjectIdAvatar(subjectId, avatar);

            return ip != null ? new IdentityPermissionData()
            {
                Id = ip.Id,
                SubjectId = ip.SubjectId,
                Avatar = ip.Avatar,
                NeuronId = ip.NeuronId,
                AccessibleLayers = ip.AccessibleLayers.Select(l => new LayerAccessData()
                {
                    NeuronId = l.NeuronId,
                    CanWrite = l.CanWrite,
                    CanRead = l.CanRead
                })
            } : null;
        }
    }
}
