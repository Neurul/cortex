using System;
using System.Collections.Generic;
using System.Text;

namespace neurUL.Cortex.Application
{
    public interface ISettingsService
    {
        string EventSourcingInBaseUrl { get; }
        string EventSourcingOutBaseUrl { get; }
    }
}
