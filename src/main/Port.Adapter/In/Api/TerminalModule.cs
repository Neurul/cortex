using CQRSlite.Commands;
using Nancy;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;

namespace org.neurul.Cortex.Port.Adapter.In.Api
{
    public class TerminalModule : NancyModule
    {
        public TerminalModule(ICommandSender commandSender) : base("/{avatarId}/cortex/terminals")
        {
            this.Put("/{terminalId}", async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        false,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            var terminalId = Guid.Parse(parameters.terminalId);
                            var avatarId = parameters.avatarId;
                            string authorId = bodyAsObject.AuthorId.ToString();

                            TerminalModule.CreateTerminalFromDynamic(bodyAsObject, out Guid presynapticNeuronId, 
                                out Guid postsynapticNeuronId, out NeurotransmitterEffect effect, out float strength);

                            return new CreateTerminal(avatarId, terminalId, presynapticNeuronId, postsynapticNeuronId, 
                                effect, strength, authorId);
                        },
                        "PresynapticNeuronId",
                        "PostsynapticNeuronId",
                        "Effect",
                        "Strength",
                        "AuthorId"
                    );
            }
            );

            this.Delete("/{terminalId}", async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new DeactivateTerminal(
                                parameters.avatarId,
                                Guid.Parse(parameters.terminalId),
                                bodyAsObject.AuthorId.ToString(),
                                expectedVersion
                                );
                        },
                        "AuthorId"
                    );
                }
            );
        }

        private static void CreateTerminalFromDynamic(dynamic dynamicTerminal, out Guid presynapticNeuronId, 
            out Guid postsynapticNeuronId, out NeurotransmitterEffect effect, out float strength)
        {
            presynapticNeuronId = Guid.Parse(dynamicTerminal.PresynapticNeuronId.ToString());
            postsynapticNeuronId = Guid.Parse(dynamicTerminal.PostsynapticNeuronId.ToString());
            string ne = dynamicTerminal.Effect.ToString();
            if (Enum.IsDefined(typeof(NeurotransmitterEffect), (int.TryParse(ne, out int ine) ? (object)ine : ne)))
                effect = (NeurotransmitterEffect)Enum.Parse(typeof(NeurotransmitterEffect), dynamicTerminal.Effect.ToString());
            else
                throw new ArgumentOutOfRangeException("Effect", $"Specified NeurotransmitterEffect value of '{dynamicTerminal.Effect.ToString()}' was invalid");
            strength = float.Parse(dynamicTerminal.Strength.ToString());
        }
    }
}
