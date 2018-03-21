// Copyright 2012,2013 Vaughn Vernon
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// Modifications copyright(C) 2017 ei8/Elmer Bool

using org.neurul.Common.Domain.Model;
using org.neurul.Common.Events;
using System;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.EventInfo
{
    public class EventInfoApplicationService : IEventInfoApplicationService
    {
        public EventInfoApplicationService(INavigableEventStore eventStore)
        {
            AssertionConcern.AssertArgumentNotNull(eventStore, nameof(eventStore));
            this.eventStore = eventStore;
        }

        readonly INavigableEventStore eventStore;

        public async Task<EventInfoLog> GetCurrentEventInfoLog()
        {
            return await new EventInfoLogFactory(this.eventStore).CreateCurrentEventInfoLog();
        }

        public async Task<EventInfoLog> GetEventInfoLog(string eventInfoLogId)
        {
            if (!EventInfoLogId.TryParse(eventInfoLogId, out EventInfoLogId logId))
                throw new FormatException($"Specified {nameof(eventInfoLogId)} value of '{eventInfoLogId}' was not in the expected format.");

            return await new EventInfoLogFactory(this.eventStore).CreateEventInfoLog(logId);
        }
    }
}
