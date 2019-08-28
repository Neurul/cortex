using System;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Domain.Model.Users
{
    public interface IUserRepository
    {
        Task<User> GetBySubjectId(Guid subjectId);

        Task Initialize(string storeId);
    }
}
