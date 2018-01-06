using CQRSlite.Events;
using Moq;
using org.neurul.Brain.Domain.Model.Neurons;
using org.neurul.Common.Events;
using org.neurul.Common.Test;
using System;
using System.Threading.Tasks;
using Xunit;

namespace org.neurul.Brain.Port.Adapter.IO.Persistence.Events.Test.LinkServiceFixture.given
{
    public abstract class Context : TestContext<LinkService>
    {
        protected Mock<INavigableEventStore> adapter;
        protected Guid gettingGuid;
        protected Guid targetGuid;
        protected bool result;

        protected override void Given()
        {
            base.Given();

            this.adapter = new Mock<INavigableEventStore>();
            this.sut = new LinkService(this.adapter.Object);
            this.targetGuid = Guid.NewGuid();

            this.adapter
                .Setup(e => e.GetLastEvent(It.IsAny<Guid>()))
                .Callback<Guid>(g => this.gettingGuid = g)
                .Returns(Task.FromResult(this.ReturnedEvent));
        }

        protected override void When()
        {
            base.When();

            Task.Run(async() => this.result = await this.sut.IsValidTarget(this.targetGuid)).Wait();
        }

        protected virtual IEvent ReturnedEvent => null;
    }
    
    public class When_validating_target
    {
        public class When_neuron_has_no_last_event : Context
        {
            protected override IEvent ReturnedEvent => null;

            [Fact]
            public void Should_get_correct_guid()
            {
                Assert.Equal(this.targetGuid, this.gettingGuid);
            }

            [Fact]
            public void Should_return_false()
            {
                Assert.False(this.result);
            }
        }

        public class When_last_event_is_not_deactivated : Context
        {
            protected override IEvent ReturnedEvent => new NeuronCreated(Guid.Empty, string.Empty);

            [Fact]
            public void Should_get_correct_guid()
            {
                Assert.Equal(this.targetGuid, this.gettingGuid);
            }

            [Fact]
            public void Should_return_true()
            {
                Assert.True(this.result);
            }
        }

        public class When_last_event_is_deactivated : Context
        {
            protected override IEvent ReturnedEvent => new NeuronDeactivated(Guid.Empty);

            [Fact]
            public void Should_get_correct_guid()
            {
                Assert.Equal(this.targetGuid, this.gettingGuid);
            }

            [Fact]
            public void Should_return_false()
            {
                Assert.False(this.result);
            }
        }
    }
}
