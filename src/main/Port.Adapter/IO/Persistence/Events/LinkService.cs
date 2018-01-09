using CQRSlite.Events;
using org.neurul.Cortex.Domain.Model.Neurons;
using org.neurul.Common.Events;
using System;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Port.Adapter.IO.Persistence.Events
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
