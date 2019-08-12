using CQRSlite.Events;
using org.neurul.Common.Events;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Port.Adapter.Common;

namespace org.neurul.Cortex.Port.Adapter.IO.Persistence.Events.SQLite
{
    public class EventStore : INavigableEventStore
    {
        private SQLiteAsyncConnection connection;
        private IEventSerializer serializer;
        private IEventPublisher publisher;

        public EventStore(IEventSerializer serializer, IEventPublisher publisher)
        {
            AssertionConcern.AssertArgumentNotNull(serializer, nameof(serializer));
            AssertionConcern.AssertArgumentNotNull(publisher, nameof(publisher));

            this.serializer = serializer;
            this.publisher = publisher;
        }

        public async Task<long> CountNotifications()
        {
            return await this.connection.Table<Notification>().CountAsync();
        }

        public void CloseConnection()
        {
            this.connection?.CloseAsync().Wait();
        }

        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            string id = aggregateId.ToString();
            // When called from CacheRepository.Get<T>, fromVersion is obtained from the AggregateRoot.Version (CQRSlite) value.
            // CacheRepository is trying to obtain only "newer" events and thus the "> fromVersion".
            var query = this.connection.Table<Notification>().Where(e => e.Id == id && e.Version > fromVersion);
            var list = await query.ToListAsync();

            return list.Select(ev => ev.ToDomainEvent(this.serializer)).ToArray();
        }

        public async Task<Notification[]> GetAllNotificationsSince(long sequenceId)
        {
            var max = await this.CountNotifications();
            AssertionConcern.AssertArgumentRange(sequenceId, 1, max, nameof(sequenceId));

            var query = this.connection.Table<Notification>().Where(e => e.SequenceId >= sequenceId);
            return (await query.ToListAsync()).ToArray();
        }

        public async Task<Notification[]> GetNotificationRange(long lowSequenceId, long highSequenceId)
        {
            AssertionConcern.AssertMinimumMaximumValid(lowSequenceId, highSequenceId, nameof(lowSequenceId), nameof(highSequenceId));
            AssertionConcern.AssertMinimum(lowSequenceId, 1, nameof(lowSequenceId));

            var query = this.connection.Table<Notification>().Where(e => e.SequenceId >= lowSequenceId && e.SequenceId <= highSequenceId);
            return (await query.ToListAsync()).ToArray();
        }

        public async Task Initialize(string storeId)
        {
            AssertionConcern.AssertArgumentNotNull(storeId, nameof(storeId));
            AssertionConcern.AssertArgumentNotEmpty(storeId, $"'{nameof(storeId)}' cannot be empty.", nameof(storeId));

            this.connection = await this.CreateConnection(storeId);
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (events.FirstOrDefault() != null && !(events.First() is IAuthoredEvent))
                throw new ArgumentException("Specified 'events' must implement IAuthoredEvent.");

            var eventData = events.Select(e => ((IAuthoredEvent) e).ToNotification(this.serializer));
            await this.connection.RunInTransactionAsync(c => c.InsertAll(eventData));
            
            events.ToList().ForEach(e => this.publisher.Publish(e, cancellationToken));            
        }

        private async Task<SQLiteAsyncConnection> CreateConnection(string storeId)
        {
            SQLiteAsyncConnection result = null;
            string databasePath = string.Format(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.DatabasePath), storeId);

            if (!databasePath.Contains(":memory:"))
                AssertionConcern.AssertPathValid(databasePath, nameof(databasePath));

            result = new SQLiteAsyncConnection(databasePath);
            await result.CreateTableAsync<Notification>();
            return result;
        }
    }
}
