﻿using CQRSlite.Events;
using org.neurul.Common.Events;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using org.neurul.Common.Domain.Model;

namespace org.neurul.Brain.Port.Adapter.IO.Persistence.Events.SQLite
{
    public class EventStore : INavigableEventStore
    {
        private SQLiteAsyncConnection connection;
        private IEventSerializer serializer;
        private IEventPublisher publisher;

        public EventStore(string databasePath, IEventSerializer serializer, IEventPublisher publisher)
        {
            AssertionConcern.AssertPathValid(databasePath, nameof(databasePath));
            AssertionConcern.AssertArgumentNotNull(serializer, nameof(serializer));
            AssertionConcern.AssertArgumentNotNull(publisher, nameof(publisher));

            this.connection = new SQLiteAsyncConnection(databasePath);
            this.serializer = serializer;
            this.publisher = publisher;
        }

        public async Task Close()
        {
            await this.connection.CloseAsync();
        }

        public async Task<long> CountEventInfo()
        {
            return await this.connection.Table<EventInfo>().CountAsync();
        }

        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            string id = aggregateId.ToString();
            // When called from CacheRepository.Get<T>, fromVersion is obtained from the AggregateRoot.Version (CQRSlite) value.
            // CacheRepository is trying to obtain only "newer" events and thus the "> fromVersion".
            var query = this.connection.Table<EventInfo>().Where(e => e.Id == id && e.Version > fromVersion);
            var list = await query.ToListAsync();

            return list.Select(ev => ev.ToDomainEvent(this.serializer)).ToArray();
        }

        public async Task<EventInfo[]> GetAllEventInfoSince(long sequenceId)
        {
            var max = await this.CountEventInfo();
            AssertionConcern.AssertArgumentRange(sequenceId, 1, max, nameof(sequenceId));

            var query = this.connection.Table<EventInfo>().Where(e => e.SequenceId >= sequenceId);
            return (await query.ToListAsync()).ToArray();
        }

        public async Task<EventInfo[]> GetEventInfoRange(long lowSequenceId, long highSequenceId)
        {
            AssertionConcern.AssertMinimumMaximumValid(lowSequenceId, highSequenceId, nameof(lowSequenceId), nameof(highSequenceId));
            AssertionConcern.AssertMinimum(lowSequenceId, 1, nameof(lowSequenceId));

            var query = this.connection.Table<EventInfo>().Where(e => e.SequenceId >= lowSequenceId && e.SequenceId <= highSequenceId);
            return (await query.ToListAsync()).ToArray();
        }

        public async Task<IEvent> GetLastEvent(Guid guid)
        {
            string id = guid.ToString();
            var query = this.connection.Table<EventInfo>().Where(e => e.Id == id);
            var result = await query.ToListAsync();

            IEvent @event = null;

            if (result.Any())
                @event = result.Last().ToDomainEvent(this.serializer);

            return @event;
        }

        public async Task Initialize()
        {
            await this.connection.CreateTableAsync<EventInfo>();
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!events.Any())
                throw new ArgumentException("Specified 'events' cannot be empty.", nameof(events));

            var eventData = events.Select(e => e.ToEventInfo(this.serializer));
            var firstEvent = events.ToArray()[0];
            var lastEvent = await this.GetLastEvent(firstEvent.Id);
            if (lastEvent != null && lastEvent.Version != firstEvent.Version - 1)
                throw new InvalidOperationException("Unexpected version of specified event.");

            await this.connection.RunInTransactionAsync(
                new Action<SQLiteConnection>(
                    c => c.InsertAll(eventData))
                );

            events.ToList().ForEach(e => this.publisher.Publish(e, cancellationToken));
        }
    }
}