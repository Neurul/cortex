using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Application
{
    public interface ISettingsService
    {
        string EventSourcingInBaseUrl { get; }
        string EventSourcingOutBaseUrl { get; }
    }
}
