using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Application.Neurons
{
    public struct Messages
    {
        public struct Exception
        {
            public const string InvalidId = "Id must not be equal to '00000000-0000-0000-0000-000000000000'.";
            public const string InvalidExpectedVersion = "Expected Version must be equal to or greater than '1'.";
        }
    }
}
