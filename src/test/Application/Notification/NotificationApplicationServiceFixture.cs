using Moq;
using org.neurul.Cortex.Application.Notification;
using org.neurul.Cortex.Domain.Model.Neurons;
using org.neurul.Cortex.Port.Adapter.IO.Persistence.Events;
using org.neurul.Common.Events;
using org.neurul.Common.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Cortex.Application.Test.Notification.NotificationApplicationServiceFixture.given
{
    public abstract class Context : TestContext<NotificationApplicationService>
    {
        protected Mock<INavigableEventStore> eventStore;
        protected List<Common.Events.Notification> events;
        protected NotificationLog log;

        protected override void Given()
        {
            base.Given();

            this.eventStore = new Mock<INavigableEventStore>();
            this.sut = new NotificationApplicationService(this.eventStore.Object);

            var s = new EventSerializer();

            this.events = this.GetEvents(s);

            this.eventStore
                .Setup(e => e.CountNotifications())
                .Returns(Task.FromResult((long)this.events.Count));

            this.eventStore
                .Setup(e => e.GetNotificationRange(It.IsAny<long>(), It.IsAny<long>()))
                .Returns<long, long>(
                    (l, h) => Task.FromResult(
                        this.events.Where(
                            e =>
                                e.SequenceId >= l &&
                                e.SequenceId <= h
                        ).ToArray()
                    )
                );
        }

        protected virtual List<Common.Events.Notification> GetEvents(IEventSerializer serializer)
        {
            var result = new List<Common.Events.Notification>();
            for (int i = 0; i < this.CountOfEventsToAdd; i++)
            {
                var ei = new NeuronCreated(Guid.NewGuid(), string.Empty, Guid.NewGuid().ToString()).ToNotification(serializer);
                ei.SequenceId = i + 1;
                result.Add(ei);
            }
            return result;
        }

        protected abstract int CountOfEventsToAdd { get; }
    }

    public class When_getting_current_log
    {
        public abstract class GettingCurrentLogContext : Context
        {
            protected override void When()
            {
                base.When();

                Task.Run(async () => this.log = await this.sut.GetCurrentNotificationLog("samplebody")).Wait();
            }
        }

        public class When_store_is_empty : GettingCurrentLogContext
        {
            protected override int CountOfEventsToAdd => 0;

            [Fact]
            public void Should_return_correct_count_of_events()
            {
                Assert.Equal(0, this.log.TotalNotification);
            }

            [Fact]
            public void Should_have_correct_log_id()
            {
                Assert.Equal("0,0", this.log.NotificationLogId);
            }

            [Fact]
            public void Should_be_null_for_first_log_id()
            {
                Assert.Null(this.log.FirstNotificationLogId);
            }

            [Fact]
            public void Should_be_null_for_previous_log_id()
            {
                Assert.Null(this.log.PreviousNotificationLogId);
            }

            [Fact]
            public void Should_be_null_for_next_log_id()
            {
                Assert.Null(this.log.NextNotificationLogId);
            }

            [Fact]
            public void Should_not_be_marked_archived()
            {
                Assert.False(this.log.IsArchived);
            }
        }

        public class When_log_size_is_greater_than_total_event_count : GettingCurrentLogContext
        {
            protected override int CountOfEventsToAdd => 15;

            [Fact]
            public void Should_return_correct_count_of_events()
            {
                Assert.Equal(15, this.log.TotalNotification);
            }

            [Fact]
            public void Should_return_correct_first_event()
            {
                Assert.Equal(1, this.log.NotificationList.First().SequenceId);
            }

            [Fact]
            public void Should_return_correct_last_event()
            {
                Assert.Equal(15, this.log.NotificationList.Last().SequenceId);
            }

            [Fact]
            public void Should_have_correct_log_id()
            {
                Assert.Equal("1,20", this.log.NotificationLogId);
            }

            [Fact]
            public void Should_have_correct_first_log_id()
            {
                Assert.Equal("1,20", this.log.FirstNotificationLogId);
            }

            [Fact]
            public void Should_have_null_previous_log_id()
            {
                Assert.Null(this.log.PreviousNotificationLogId);
            }

            [Fact]
            public void Should_not_be_marked_archived()
            {
                Assert.False(this.log.IsArchived);
            }

            [Fact]
            public void Should_be_null_for_next_id()
            {
                Assert.Null(this.log.NextNotificationLogId);
            }
        }

        public class When_log_size_is_less_than_total_event_count : GettingCurrentLogContext
        {
            protected override int CountOfEventsToAdd => 30;

            [Fact]
            public void Should_return_correct_count_of_events_in_excess_of_log_size()
            {
                Assert.Equal(10, this.log.TotalNotification);
            }

            [Fact]
            public void Should_return_correct_first_event()
            {
                Assert.Equal(21, this.log.NotificationList.First().SequenceId);
            }

            [Fact]
            public void Should_return_correct_last_event()
            {
                Assert.Equal(30, this.log.NotificationList.Last().SequenceId);
            }

            [Fact]
            public void Should_have_correct_log_id()
            {
                Assert.Equal("21,40", this.log.NotificationLogId);
            }

            [Fact]
            public void Should_have_correct_first_log_id()
            {
                Assert.Equal("1,20", this.log.FirstNotificationLogId);
            }

            [Fact]
            public void Should_have_correct_previous_log_id()
            {
                Assert.Equal("1,20", this.log.PreviousNotificationLogId);
            }

            [Fact]
            public void Should_not_be_marked_archived()
            {
                Assert.False(this.log.IsArchived);
            }

            [Fact]
            public void Should_be_null_for_next_id()
            {
                Assert.Null(this.log.NextNotificationLogId);
            }
        }

        public class When_log_size_is_less_than_half_of_total_event_count : GettingCurrentLogContext
        {
            protected override int CountOfEventsToAdd => 45;

            [Fact]
            public void Should_return_correct_count_of_events_in_excess_of_log_size()
            {
                Assert.Equal(5, this.log.TotalNotification);
            }

            [Fact]
            public void Should_return_correct_first_event()
            {
                Assert.Equal(41, this.log.NotificationList.First().SequenceId);
            }

            [Fact]
            public void Should_return_correct_last_event()
            {
                Assert.Equal(45, this.log.NotificationList.Last().SequenceId);
            }

            [Fact]
            public void Should_have_correct_log_id()
            {
                Assert.Equal("41,60", this.log.NotificationLogId);
            }

            [Fact]
            public void Should_have_correct_first_log_id()
            {
                Assert.Equal("1,20", this.log.FirstNotificationLogId);
            }

            [Fact]
            public void Should_have_correct_previous_log_id()
            {
                Assert.Equal("21,40", this.log.PreviousNotificationLogId);
            }

            [Fact]
            public void Should_not_be_marked_archived()
            {
                Assert.False(this.log.IsArchived);
            }

            [Fact]
            public void Should_be_null_for_next_id()
            {
                Assert.Null(this.log.NextNotificationLogId);
            }
        }

        public class When_log_size_is_equal_to_total_event_count : GettingCurrentLogContext
        {
            protected override int CountOfEventsToAdd => 20;

            [Fact]
            public void Should_return_correct_count_of_events()
            {
                Assert.Equal(20, this.log.TotalNotification);
            }

            [Fact]
            public void Should_return_correct_first_event()
            {
                Assert.Equal(1, this.log.NotificationList.First().SequenceId);
            }

            [Fact]
            public void Should_return_correct_last_event()
            {
                Assert.Equal(20, this.log.NotificationList.Last().SequenceId);
            }

            [Fact]
            public void Should_have_correct_log_id()
            {
                Assert.Equal("1,20", this.log.NotificationLogId);
            }

            [Fact]
            public void Should_have_correct_first_log_id()
            {
                Assert.Equal("1,20", this.log.FirstNotificationLogId);
            }

            [Fact]
            public void Should_have_null_previous_log_id()
            {
                Assert.Null(this.log.PreviousNotificationLogId);
            }

            [Fact]
            public void Should_be_marked_archived()
            {
                Assert.True(this.log.IsArchived);
            }

            [Fact]
            public void Should_be_null_for_next_id()
            {
                Assert.Null(this.log.NextNotificationLogId);
            }
        }

        public class When_total_event_count_is_divisible_by_log_size : GettingCurrentLogContext
        {      
            protected override int CountOfEventsToAdd => 60;

            [Fact]
            public void Should_return_correct_count_of_events()
            {
                Assert.Equal(20, this.log.TotalNotification);
            }

            [Fact]
            public void Should_return_correct_first_event()
            {
                Assert.Equal(41, this.log.NotificationList.First().SequenceId);
            }

            [Fact]
            public void Should_return_correct_last_event()
            {
                Assert.Equal(60, this.log.NotificationList.Last().SequenceId);
            }

            [Fact]
            public void Should_have_correct_log_id()
            {
                Assert.Equal("41,60", this.log.NotificationLogId);
            }

            [Fact]
            public void Should_have_correct_first_log_id()
            {
                Assert.Equal("1,20", this.log.FirstNotificationLogId);
            }

            [Fact]
            public void Should_have_correct_previous_log_id()
            {
                Assert.Equal("21,40", this.log.PreviousNotificationLogId);
            }

            [Fact]
            public void Should_be_marked_archived()
            {
                Assert.True(this.log.IsArchived);
            }

            [Fact]
            public void Should_be_null_for_next_id()
            {
                Assert.Null(this.log.NextNotificationLogId);
            }
        }
    }

    public class When_getting_log
    {
        public abstract class GettingLogContext : Context
        {
            protected override void When()
            {
                base.When();

                Task.Run(async () => this.log = await this.sut.GetNotificationLog("samplebody", this.NotificationLogId)).Wait();
            }

            protected abstract string NotificationLogId { get; }
        }

        public class When_specified_id_format_is_invalid : GettingLogContext
        {
            protected override int CountOfEventsToAdd => 1;

            protected override string NotificationLogId => "15";

            protected override bool InvokeWhenOnConstruct => false;

            [Fact]
            public async Task Should_throw_format_exception()
            {
                await Assert.ThrowsAsync<FormatException>(() => this.sut.GetNotificationLog("samplebody", this.NotificationLogId));
            }
        }

        public class When_store_is_empty : GettingLogContext
        {
            protected override int CountOfEventsToAdd => 0;

            protected override string NotificationLogId => "1,20";

            [Fact]
            public void Should_return_correct_count_of_events()
            {
                Assert.Equal(0, this.log.TotalNotification);
            }

            [Fact]
            public void Should_have_correct_log_id()
            {
                Assert.Equal("1,20", this.log.NotificationLogId);
            }

            [Fact]
            public void Should_be_null_for_first_log_id()
            {
                Assert.Null(this.log.FirstNotificationLogId);
            }

            [Fact]
            public void Should_be_null_for_previous_log_id()
            {
                Assert.Null(this.log.PreviousNotificationLogId);
            }

            [Fact]
            public void Should_be_null_for_next_log_id()
            {
                Assert.Null(this.log.NextNotificationLogId);
            }

            [Fact]
            public void Should_not_be_marked_archived()
            {
                Assert.False(this.log.IsArchived);
            }
        }

// Store contains events
// ======================
//                      requested   returned        previous    next                                valid
//case      totcount    min max     min max count   min max     min max     archived    exception   high    low     description
//1         60          41  60      41  60  20      21  40      null        true        false       /       /       
//2         60          21  40      21  40  20      1   20      41  60      true        false       /       /
//3         60          1   20      1   20  20      null        21  40      true        false       /       /
//4         60          61  80      61  80  0       41  60      null        false       false       /       /
//5         60          81  100     81  100 0       null        null        false       false       /       /
//6         21		    1	20		1   20  20      null   		21	40      true        false       /       /
//7         21          21  40      21  40  1       1   20      null        false       false       /       /
//8         60		    45	60		                                                    true        /
//9         30          1   16                                                          true                /
//10        30          21  30                                                          true                /
//11        30          0   5                                                           true        
//12        30          16  25		                                                    true        
//13        60          16  25                                                          true        

        public class When_store_contains_events
        {
            public class When_requested_id_is_valid
            {
                public class When_case_1 : GettingLogContext
                {
                    protected override int CountOfEventsToAdd => 60;

                    protected override string NotificationLogId => "41,60";

                    [Fact]
                    public void Should_have_correct_first_log_id()
                    {
                        Assert.Equal("1,20", this.log.FirstNotificationLogId);
                    }

                    [Fact]
                    public void Should_have_correct_event_id()
                    {
                        Assert.Equal("41,60", this.log.NotificationLogId);
                    }

                    [Fact]
                    public void Should_return_correct_number_of_events()
                    {
                        Assert.Equal(20, this.log.TotalNotification);
                    }                    

                    [Fact]
                    public void Should_be_correct_previous_id()
                    {
                        Assert.Equal("21,40", this.log.PreviousNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_null_for_next_id()
                    {
                        Assert.Null(this.log.NextNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_marked_archived()
                    {
                        Assert.True(this.log.IsArchived);
                    }
                }

                public class When_case_2 : GettingLogContext
                {
                    protected override int CountOfEventsToAdd => 60;

                    protected override string NotificationLogId => "21,40";

                    [Fact]
                    public void Should_have_correct_first_log_id()
                    {
                        Assert.Equal("1,20", this.log.FirstNotificationLogId);
                    }

                    [Fact]
                    public void Should_have_correct_event_id()
                    {
                        Assert.Equal("21,40", this.log.NotificationLogId);
                    }

                    [Fact]
                    public void Should_return_correct_number_of_events()
                    {
                        Assert.Equal(20, this.log.TotalNotification);
                    }

                    [Fact]
                    public void Should_be_correct_previous_id()
                    {
                        Assert.Equal("1,20", this.log.PreviousNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_correct_next_id()
                    {
                        Assert.Equal("41,60", this.log.NextNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_marked_archived()
                    {
                        Assert.True(this.log.IsArchived);
                    }
                }

                public class When_case_3 : GettingLogContext
                {
                    protected override int CountOfEventsToAdd => 60;

                    protected override string NotificationLogId => "1,20";

                    [Fact]
                    public void Should_have_correct_first_log_id()
                    {
                        Assert.Equal("1,20", this.log.FirstNotificationLogId);
                    }

                    [Fact]
                    public void Should_have_correct_event_id()
                    {
                        Assert.Equal("1,20", this.log.NotificationLogId);
                    }

                    [Fact]
                    public void Should_return_correct_number_of_events()
                    {
                        Assert.Equal(20, this.log.TotalNotification);
                    }

                    [Fact]
                    public void Should_be_null_for_previous_id()
                    {
                        Assert.Null(this.log.PreviousNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_correct_next_id()
                    {
                        Assert.Equal("21,40", this.log.NextNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_marked_archived()
                    {
                        Assert.True(this.log.IsArchived);
                    }
                }

                public class When_case_4 : GettingLogContext
                {
                    protected override int CountOfEventsToAdd => 60;

                    protected override string NotificationLogId => "61,80";

                    [Fact]
                    public void Should_have_correct_first_log_id()
                    {
                        Assert.Equal("1,20", this.log.FirstNotificationLogId);
                    }

                    [Fact]
                    public void Should_have_correct_event_id()
                    {
                        Assert.Equal("61,80", this.log.NotificationLogId);
                    }

                    [Fact]
                    public void Should_return_correct_number_of_events()
                    {
                        Assert.Equal(0, this.log.TotalNotification);
                    }

                    [Fact]
                    public void Should_be_correct_previous_id()
                    {
                        Assert.Equal("41,60", this.log.PreviousNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_null_for_next_id()
                    {
                        Assert.Null(this.log.NextNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_marked_archived()
                    {
                        Assert.False(this.log.IsArchived);
                    }
                }

                public class When_case_5 : GettingLogContext
                {
                    protected override int CountOfEventsToAdd => 60;

                    protected override string NotificationLogId => "81,100";

                    [Fact]
                    public void Should_have_correct_first_log_id()
                    {
                        Assert.Equal("1,20", this.log.FirstNotificationLogId);
                    }

                    [Fact]
                    public void Should_have_correct_event_id()
                    {
                        Assert.Equal("81,100", this.log.NotificationLogId);
                    }

                    [Fact]
                    public void Should_return_correct_number_of_events()
                    {
                        Assert.Equal(0, this.log.TotalNotification);
                    }

                    [Fact]
                    public void Should_be_null_for_previous_id()
                    {
                        Assert.Null(this.log.PreviousNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_null_for_next_id()
                    {
                        Assert.Null(this.log.NextNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_marked_archived()
                    {
                        Assert.False(this.log.IsArchived);
                    }
                }

                public class When_case_6 : GettingLogContext
                {
                    protected override int CountOfEventsToAdd => 21;

                    protected override string NotificationLogId => "1,20";

                    [Fact]
                    public void Should_have_correct_first_log_id()
                    {
                        Assert.Equal("1,20", this.log.FirstNotificationLogId);
                    }

                    [Fact]
                    public void Should_have_correct_event_id()
                    {
                        Assert.Equal("1,20", this.log.NotificationLogId);
                    }

                    [Fact]
                    public void Should_return_correct_number_of_events()
                    {
                        Assert.Equal(20, this.log.TotalNotification);
                    }

                    [Fact]
                    public void Should_be_null_previous_id()
                    {
                        Assert.Null(this.log.PreviousNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_correct_next_id()
                    {
                        Assert.Equal("21,40", this.log.NextNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_marked_archived()
                    {
                        Assert.True(this.log.IsArchived);
                    }
                }

                public class When_case_7 : GettingLogContext
                {
                    protected override int CountOfEventsToAdd => 21;

                    protected override string NotificationLogId => "21,40";

                    [Fact]
                    public void Should_have_correct_first_log_id()
                    {
                        Assert.Equal("1,20", this.log.FirstNotificationLogId);
                    }

                    [Fact]
                    public void Should_have_correct_event_id()
                    {
                        Assert.Equal("21,40", this.log.NotificationLogId);
                    }

                    [Fact]
                    public void Should_return_correct_number_of_events()
                    {
                        Assert.Equal(1, this.log.TotalNotification);
                    }

                    [Fact]
                    public void Should_have_correct_previous_id()
                    {
                        Assert.Equal("1,20", this.log.PreviousNotificationLogId);
                    }

                    [Fact]
                    public void Should_be_null_for_next_id()
                    {
                        Assert.Null(this.log.NextNotificationLogId);
                    }

                    [Fact]
                    public void Should_not_be_marked_archived()
                    {
                        Assert.False(this.log.IsArchived);
                    }
                }
            }

            public class When_requested_id_is_invalid
            {
                public class When_case_8 : GettingLogContext
                {
                    protected override bool InvokeWhenOnConstruct => false;

                    protected override int CountOfEventsToAdd => 60;

                    protected override string NotificationLogId => "45,60";

                    [Fact]
                    public async Task Should_throw_argument_exception()
                    {
                        await Assert.ThrowsAsync<ArgumentException>("notificationLogId", () => this.sut.GetNotificationLog("samplebody", this.NotificationLogId));
                    }
                }

                public class When_case_9 : GettingLogContext
                {
                    protected override bool InvokeWhenOnConstruct => false;

                    protected override int CountOfEventsToAdd => 30;

                    protected override string NotificationLogId => "1,16";

                    [Fact]
                    public async Task Should_throw_argument_exception()
                    {
                        await Assert.ThrowsAsync<ArgumentException>("notificationLogId", () => this.sut.GetNotificationLog("samplebody", this.NotificationLogId));
                    }
                }

                public class When_case_10 : GettingLogContext
                {
                    protected override bool InvokeWhenOnConstruct => false;

                    protected override int CountOfEventsToAdd => 30;

                    protected override string NotificationLogId => "21,30";

                    [Fact]
                    public async Task Should_throw_argument_exception()
                    {
                        await Assert.ThrowsAsync<ArgumentException>("notificationLogId", () => this.sut.GetNotificationLog("samplebody", this.NotificationLogId));
                    }
                }

                public class When_case_11 : GettingLogContext
                {
                    protected override bool InvokeWhenOnConstruct => false;

                    protected override int CountOfEventsToAdd => 30;

                    protected override string NotificationLogId => "0,5";

                    [Fact]
                    public async Task Should_throw_argument_exception()
                    {
                        await Assert.ThrowsAsync<ArgumentException>("notificationLogId", () => this.sut.GetNotificationLog("samplebody", this.NotificationLogId));
                    }
                }

                public class When_case_12 : GettingLogContext
                {
                    protected override bool InvokeWhenOnConstruct => false;

                    protected override int CountOfEventsToAdd => 30;

                    protected override string NotificationLogId => "16,25";

                    [Fact]
                    public async Task Should_throw_argument_exception()
                    {
                        await Assert.ThrowsAsync<ArgumentException>("notificationLogId", () => this.sut.GetNotificationLog("samplebody", this.NotificationLogId));
                    }
                }

                public class When_case_13: GettingLogContext
                {
                    protected override bool InvokeWhenOnConstruct => false;

                    protected override int CountOfEventsToAdd => 60;

                    protected override string NotificationLogId => "16,25";

                    [Fact]
                    public async Task Should_throw_argument_exception()
                    {
                        await Assert.ThrowsAsync<ArgumentException>("notificationLogId", () => this.sut.GetNotificationLog("samplebody", this.NotificationLogId));
                    }
                }
            }
        }
    }
}
