using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Brain.Domain.Model.Neurons
{
    public interface ILinkService
    {
        Task<bool> IsValidTarget(Guid guid);
    }
}
