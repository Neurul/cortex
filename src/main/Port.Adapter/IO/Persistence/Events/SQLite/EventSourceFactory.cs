using CQRSlite.Domain;
using CQRSlite.Events;
using org.neurul.Common.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Port.Adapter.IO.Persistence.Events.SQLite
{
    public class EventSourceFactory : IEventSourceFactory
    {
        private IEventSerializer serializer;
        private IEventPublisher publisher;

        public EventSourceFactory(IEventSerializer serializer, IEventPublisher publisher)
        {
            this.serializer = serializer;
            this.publisher = publisher;
        }

        public EventSource CreateEventSource(string storeId)
        {
            var es = new EventStore(storeId, this.serializer, this.publisher);
            var r = new Repository(es);
            var s = new Session(r);
            return new EventSource(storeId, s, r, es);
        }
    }
}
