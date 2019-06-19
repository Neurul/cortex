using System;
using System.Collections.Generic;
using System.Text;

namespace org.neurul.Cortex.Domain.Model.Neurons
{
    public enum NeurotransmitterEffect
    {
        Inhibit = -1,
        NotSet = 0,
        Excite = 1
    }

    public struct Messages
    {
        public struct Exception
        {
            public const string IdEmpty = "Specified Id cannot be empty.";
            public const string NeuronInactive = "Neuron must be active.";
            public const string TerminalInactive = "Terminal must be active.";
            public const string NonAuthorConstructor = "Cannot create Author Neuron using current constructor. Use Author Neuron constructor overload instead.";
            public const string InvalidTerminalIdCreation = "Terminal Id cannot be equal to Author Id";
            public const string ValidEffect = "Effect must either be Excite or Inhibit";
            public const string StrengthInvalid = "Strength must be greater than 0, and less than or equal to 1.";
            public const string PostCannotBeTheSameAsPre = "Postsynaptic Neuron cannot be the same as Presynaptic Neuron.";
        }
    }
}
