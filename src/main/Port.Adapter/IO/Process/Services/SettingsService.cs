using org.neurul.Cortex.Application;
using org.neurul.Cortex.Port.Adapter.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Port.Adapter.IO.Process.Services
{
    public class SettingsService : ISettingsService
    {
        public string EventSourcingInBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.EventSourcingInBaseUrl);

        public string EventSourcingOutBaseUrl => Environment.GetEnvironmentVariable(EnvironmentVariableKeys.EventSourcingOutBaseUrl);
    }
}
