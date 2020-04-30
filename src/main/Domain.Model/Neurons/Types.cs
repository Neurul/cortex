using System;
using System.Collections.Generic;
using System.Text;

namespace neurUL.Cortex.Domain.Model.Neurons
{
    public struct Messages
    {
        public struct Exception
        {
            public const string IdEmpty = "Specified Id cannot be empty.";
            public const string NeuronInactive = "Neuron must be active.";
            public const string TerminalInactive = "Terminal must be active.";
            public const string InvalidTerminalIdCreation = "Terminal Id cannot be equal to Neuron Id";
            public const string ValidEffect = "Effect must either be Excite or Inhibit";
            public const string StrengthInvalid = "Strength must be greater than 0, and less than or equal to 1.";
            public const string PostCannotBeTheSameAsPre = "Postsynaptic Neuron cannot be the same as Presynaptic Neuron.";
            public const string InvalidNeuronSpecified = "Specified Neuron with Id '{0}' did not match expected with Id '{1}'.";
        }
    }
}
