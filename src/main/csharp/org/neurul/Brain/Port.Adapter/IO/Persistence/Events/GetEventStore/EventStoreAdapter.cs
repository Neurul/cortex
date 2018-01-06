using org.neurul.Common.Port.Adapters.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using CQRSlite.Events;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using System.Linq;

namespace org.neurul.Brain.Port.Adapter.IO.Persistence.Events.GetEventStore
{
    public class EventStoreAdapter : IEventStoreAdapter
    {
        private IEventStoreConnection connection;
        private IEventSerializer serializer;

        public EventStoreAdapter(string connectionString, IEventSerializer serializer)
        {
            this.connection = EventStoreConnection.Create(connectionString);
            this.serializer = serializer;
        }

        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion)
        {
            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;

            // When called from CacheRepository.Get<T>, fromVersion is obtained from the AggregateRoot.Version (CQRSlite) value.
            // And since it is one version (See "EventStore.Save" method note) more than the EventStore version value, 
            // this means that the CacheRepository is trying to obtain only "newer" events.
            var nextSliceStart = (long)(fromVersion - 1 < 0 ? StreamPosition.Start : fromVersion);
            do
            {
                currentSlice = await this.connection.ReadStreamEventsForwardAsync(
                    EventStoreAdapter.GetStreamName(aggregateId),
                    nextSliceStart,
                    200,
                    false).ConfigureAwait(false);

                nextSliceStart = currentSlice.NextEventNumber;
                streamEvents.AddRange(currentSlice.Events);

            } while (!currentSlice.IsEndOfStream);

            return currentSlice.Events.Select(ev => {
                return this.serializer.Deserialize(Encoding.UTF8.GetString(ev.Event.Metadata), Encoding.UTF8.GetString(ev.Event.Data));
            });
        }

        private static string GetStreamName(Guid id)
        {
            return "Neuron-" + id.ToString();
        }

        public async Task Save(Func<IEvent, EventMetadata> metadataGenerator, IEnumerable<IEvent> events)
        {
            var eventData = events.Select<IEvent, EventData>((e, ed) =>
            {
                var contentJson = this.serializer.Serialize(e);
                var metaDataJson = this.serializer.Serialize(metadataGenerator(e));

                if (string.IsNullOrEmpty(contentJson))
                    throw new InvalidOperationException("Failed deserializing event.");
                if (string.IsNullOrEmpty(metaDataJson))
                    throw new InvalidOperationException("Failed deserializing event metadata.");

                return new EventData(
                        Guid.NewGuid(),
                        e.GetType().Name,
                        isJson: true,
                        data: Encoding.UTF8.GetBytes(contentJson),
                        metadata: Encoding.UTF8.GetBytes(metaDataJson)
                    );
            });

            // TODO: ensure no crash
            var firstEvent = events.ToArray()[0];

            await connection.AppendToStreamAsync(
                EventStoreAdapter.GetStreamName(firstEvent.Id),
                // CQRSlite framework starts versioning with 1
                // EventStore starts versioning with 0 (ESVersion = CQRSliteVersion - 1)
                // ExpectedVersion (of Aggregate in EventStore)
                //      parameter starts with -1 (ExpectedVersion.NoStream)
                //      = NewEvent.Version - 2 (version increment of 1 + difference of CQRSlite with EventStore of 1)
                firstEvent.Version - 2,
                eventData
                );            
        }

        public async Task<IEvent> GetLastEvent(Guid guid)
        {
            var slice = await this.connection.ReadStreamEventsBackwardAsync(
                       EventStoreAdapter.GetStreamName(guid),
                       StreamPosition.End,
                       1,
                       false
                       ).ConfigureAwait(false);

            IEvent @event = null;

            if (slice.Events.Any())
            {
                var last = slice.Events.Last();
                @event = this.serializer.Deserialize(Encoding.UTF8.GetString(last.Event.Metadata), Encoding.UTF8.GetString(last.Event.Data));
            }

            return @event;
        }

        public async Task Initialize()
        {
            await this.connection.ConnectAsync();
        }
    }
}
