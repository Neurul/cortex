using CQRSlite.Events;
using Newtonsoft.Json;
using org.neurul.Common.Events;
using System;

namespace org.neurul.Cortex.Port.Adapter.IO.Persistence.Events
{
    public class EventSerializer : IEventSerializer
    {
        public IAuthoredEvent Deserialize(string typeName, string eventData)
        {
            var eventType = default(Type);
            try
            {
                eventType = Type.GetType(typeName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format("Type load error, reason: {0}", ex));
            }
            return (IAuthoredEvent)JsonConvert.DeserializeObject(eventData, eventType);
        }

        public string Serialize(IAuthoredEvent @event)
        {
            return JsonConvert.SerializeObject(@event);
        }
    }
}
