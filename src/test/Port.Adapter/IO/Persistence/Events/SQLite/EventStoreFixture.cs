using CQRSlite.Events;
using Moq;
using org.neurul.Cortex.Domain.Model.Neurons;
using org.neurul.Common.Events;
using org.neurul.Common.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SQLite;

namespace org.neurul.Cortex.Port.Adapter.IO.Persistence.Events.SQLite.Test.EventStoreFixture.given
{
    public class When_constructing
    { 
        public class When_null_adapter_is_specified : TestContext<EventStore>
        {
            [Fact]
            public void Should_throw_argument_null_exception()
            {
                var ep = new Mock<IEventPublisher>().Object;
                Assert.Throws<ArgumentNullException>("serializer", () => new EventStore(null, ep));
            }
        }

        public class When_null_publisher_is_specified : TestContext<EventStore>
        {
            [Fact]
            public void Should_throw_argument_null_exception()
            {
                var esa = new Mock<IEventSerializer>().Object;
                Assert.Throws<ArgumentNullException>("publisher", () => new EventStore(esa, null));
            }
        }
    }

    public abstract class ProperlyConstructedContext : TestContext<EventStore>
    {
        protected IEventSerializer serializer;
        protected Mock<IEventPublisher> publisher;

        protected override void Given()
        {
            base.Given();
            this.GivenInit();

            this.publisher
                    .Setup(e => e.Publish(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()))
                    .Callback(this.PublishEventCallback)
                    .Returns(Task.CompletedTask);
        }

        protected virtual void GivenInit()
        {
            this.serializer = new EventSerializer();
            this.publisher = new Mock<IEventPublisher>();
            this.sut = new EventStore(serializer, publisher.Object);            
        }

        protected virtual Action<IEvent, CancellationToken> PublishEventCallback => (e, c) => {};
    }

    public class When_initializing
    {
        public class When_null_store_id_is_specified : ProperlyConstructedContext
        {
            [Fact]
            public async void Should_throw_null_exception()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() => this.sut.Initialize(null));
            }
        }

        public class When_empty_store_id_is_specified : ProperlyConstructedContext
        {
            [Fact]
            public async void Should_throw_argument_exception()
            {
                await Assert.ThrowsAsync<ArgumentException>(() => this.sut.Initialize(string.Empty));
            }
        }
    }

    public abstract class InitializedContext : ProperlyConstructedContext
    {
        protected override void GivenInit()
        {
            base.GivenInit();
            Environment.SetEnvironmentVariable("DatabasePath", ":{0}:");            
            Task.Run(() => this.sut.Initialize("memory")).Wait();
            //string databasePath = string.Format(Environment.GetEnvironmentVariable("DatabasePath"), "memory");
            //using (var c = new SQLiteConnection(databasePath))
            //{
            //    c.DeleteAll<Notification>();
            //}
        }
    }

    public class When_saving_event 
    {
        public class When_store_is_empty : InitializedContext
        {
            private IEnumerable<IEvent> events;
            private List<IEvent> publishedEventsList;
            private Guid guid;
            private string authorId;

            protected override void GivenInit()
            {
                base.GivenInit();

                this.guid = Guid.NewGuid();
                this.authorId = Guid.NewGuid().ToString();

                this.events = new List<IEvent>() {
                    new NeuronCreated(this.guid, string.Empty, this.authorId) { Version = 1 }
                };

                this.publishedEventsList = new List<IEvent>();
            }

            protected override Action<IEvent, CancellationToken> PublishEventCallback => (e, c) => this.publishedEventsList.Add(e);

            protected override void When()
            {
                base.When();

                Task.Run(async () => await this.sut.Save(this.events)).Wait();
            }

            [Fact]
            public async Task Should_contain_correct_typename_prefixes()
            {
                var events = await this.sut.Get(this.guid, 0);
                Assert.StartsWith("org.neurul.Cortex.Domain.Model", ((IAuthoredEvent) events.Last()).ToNotification(this.serializer).TypeName);
            }

            [Fact]
            public async Task Should_contain_correct_typename()
            {
                var events = await this.sut.Get(this.guid, 0);
                Assert.StartsWith("org.neurul.Cortex.Domain.Model.Neurons.NeuronCreated", ((IAuthoredEvent) events.Last()).ToNotification(this.serializer).TypeName);
            }

            [Fact]
            public void Should_publish_event()
            {
                Assert.Single(this.publishedEventsList);
            }
        }
    }

    public abstract class ContainingEventsContext : InitializedContext, IDisposable
    {
        protected List<Notification> notificationList;
        protected IEventSerializer eventSerializer;
        protected long sequenceId;

        protected override void GivenInit()
        {
            base.GivenInit();

            this.sequenceId = 1;
            this.notificationList = new List<Notification>();
            this.eventSerializer = new EventSerializer();
        }

        protected override void Given()
        {
            base.Given();

            Task.Run(async () => await this.sut.Save(new IEvent[] {
                    new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString()),
                    new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString()),
                    new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString()),
                    new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString()),
                    new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString()),
                    new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString()),
                    new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString()),
                    new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString()),
                    new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString())
                }
            )).Wait();
        }

        public void Dispose()
        {
            this.sut.Dispose();
        }
    }

    public class When_counting_events
    {
        public class When_store_contains_events : ContainingEventsContext
        {
            private long count;

            protected override void When()
            {
                base.When();

                Task.Run(async () => this.count = await this.sut.CountNotifications()).Wait();
            }

            [Fact]
            public void Should_be_equal_to_nine()
            {
                Assert.Equal(9, this.count);
            }
        }
    }

    public class When_getting_event_range
    {
        public abstract class GettingRangeContext : ContainingEventsContext
        {
            protected Notification[] resultNotification;

            protected override void When()
            {
                base.When();

                Task.Run(async () => 
                    this.resultNotification = await this.sut.GetNotificationRange(this.LowSequenceId, this.HighSequenceId)
                ).Wait();
            }

            protected abstract long LowSequenceId { get; }

            protected abstract long HighSequenceId { get; }
        }

        public class When_store_contains_events
        {
            public class When_low_sequence_id_is_greater_than_high_sequence_id : ContainingEventsContext
            {
                [Fact]
                public async Task Should_throw_argument_exception()
                {
                    await Assert.ThrowsAsync<ArgumentException>("lowSequenceId", async () => await this.sut.GetNotificationRange(2, 1));
                }
            }

            public class When_high_sequence_id_is_greater_than_count : GettingRangeContext
            {
                protected override long LowSequenceId => 6;

                protected override long HighSequenceId => 10;

                [Fact]
                public void Should_return_all_events_that_can_be_returned()
                {
                    Assert.Equal(4, this.resultNotification.Length);
                }
            }

            public class When_low_sequence_id_is_less_than_absolute_minimum : ContainingEventsContext
            {
                [Fact]
                public async Task Should_throw_argument_out_of_range_exception()
                {
                    await Assert.ThrowsAsync<ArgumentOutOfRangeException>("lowSequenceId", async () => await this.sut.GetNotificationRange(0, 9));
                }
            }

            public class When_specified_arguments_are_valid : ContainingEventsContext
            {
                [Fact]
                public async Task Should_return_correct_count_of_events()
                {
                    Assert.Equal(5, (await this.sut.GetNotificationRange(4, 8)).Count());
                }

                [Fact]
                public async Task Should_return_first_event_with_correct_sequence_id()
                {
                    Assert.Equal(4, (await this.sut.GetNotificationRange(4, 8)).First().SequenceId);
                }

                [Fact]
                public async Task Should_return_last_event_with_correct_sequence_id()
                {
                    Assert.Equal(8, (await this.sut.GetNotificationRange(4, 8)).Last().SequenceId);
                }
            }
        }
    }

    public class When_getting_all_events_since_sequence_id
    {
        public class When_store_contains_events
        {
            public class When_specified_sequence_id_is_less_than_absolute_minimum : ContainingEventsContext
            {
                [Fact]
                public async Task Should_throw_argument_out_of_range_exception()
                {
                    await Assert.ThrowsAsync<ArgumentOutOfRangeException>("sequenceId", async () => await this.sut.GetAllNotificationsSince(0));
                }
            }

            public class When_specified_sequence_id_is_greater_than_count : ContainingEventsContext
            {
                [Fact]
                public async Task Should_throw_argument_out_of_range_exception()
                {
                    await Assert.ThrowsAsync<ArgumentOutOfRangeException>("sequenceId", async () => await this.sut.GetAllNotificationsSince(10));
                }
            }

            public class When_specified_arguments_are_valid : ContainingEventsContext
            {
                [Fact]
                public async Task Should_return_correct_count_of_events()
                {
                    Assert.Equal(5, (await this.sut.GetAllNotificationsSince(5)).Count());
                }
            }
        }
    }
}
