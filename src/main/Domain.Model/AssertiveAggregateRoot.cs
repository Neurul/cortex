using CQRSlite.Domain;
using CQRSlite.Events;
using org.neurul.Common.Domain.Model;
using System.Collections.Generic;

namespace org.neurul.Cortex.Domain.Model
{
    public abstract class AssertiveAggregateRoot : AggregateRoot
    {
        private static readonly IList<string> cache = new List<string>();

        protected override void ApplyEvent(IEvent @event)
        {
            var aggregateType = this.GetType();
            var eventType = @event.GetType();
            var eventName = eventType.Name;
            var typeEventName = aggregateType.Name + eventName;
            if (!AssertiveAggregateRoot.cache.Contains(typeEventName))
            {
                // check if event is supported
                var argtypes = Helper.GetArgTypes(new object[] { @event });
                var m = Helper.GetMember(aggregateType, "Apply", argtypes);

                // actual fix should be in line 34 of https://github.com/gautema/CQRSlite/blob/master/Framework/CQRSlite/Infrastructure/DynamicInvoker.cs
                AssertionConcern.AssertStateTrue(m != null, $"'{eventName}' is not a recognized event of '{aggregateType.Name}'.");

                AssertiveAggregateRoot.cache.Add(typeEventName);
            }
            base.ApplyEvent(@event);
        }
    }
}
