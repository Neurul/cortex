using CQRSlite.Events;
using org.neurul.Brain.Domain.Model.Neurons;
using org.neurul.Common.Events;
using System;
using System.Threading.Tasks;

namespace org.neurul.Brain.Port.Adapter.IO.Persistence.Events
{
    public class LinkService : ILinkService
    {
        private INavigableEventStore adapter;

        public LinkService(INavigableEventStore adapter)
        {
            this.adapter = adapter;
        }

        public async Task<bool> IsValidTarget(Guid guid)
        {
            IEvent lastEvent = await this.adapter.GetLastEvent(guid);
            return lastEvent != null && !(lastEvent is NeuronDeactivated);
        }
    }
}
