using Common.Test;
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using Moq;
using org.neurul.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.Test.Neurons
{
    public abstract class ConstructingContext<TAggregate, THandler, TCommand> : ConditionalWhenSpecification<TAggregate, THandler, TCommand> where TAggregate : AggregateRoot where THandler : class where TCommand : ICommand
    {
        protected INavigableEventStore eventStore;

        protected override IEnumerable<IEvent> Given() => new IEvent[0];
        protected override TCommand When() => default(TCommand);

        protected virtual INavigableEventStore EventStore => this.eventStore = this.eventStore ?? new Mock<INavigableEventStore>().Object;
        protected override bool InvokeBuildWhenOnConstruct => false;
    }
}
