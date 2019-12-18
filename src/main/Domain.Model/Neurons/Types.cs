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
            public const string InvalidTerminalIdCreation = "Terminal Id cannot be equal to Neuron Id";
            public const string ValidEffect = "Effect must either be Excite or Inhibit";
            public const string StrengthInvalid = "Strength must be greater than 0, and less than or equal to 1.";
            public const string PostCannotBeTheSameAsPre = "Postsynaptic Neuron cannot be the same as Presynaptic Neuron.";
            // TODO: transfer to Sentry
            //public const string UnauthorizedLayerWriteTemplate = "User must be authorized to write to Layer '{0}'.";
            //public const string UnauthorizedNeuronWriteTemplate = "User must be Layer Admin or Neuron Creator to modify Neuron '{0}'.";
            public const string InvalidNeuronSpecified = "Specified Neuron with Id '{0}' did not match expected with Id '{1}'.";
        }
    }
}
