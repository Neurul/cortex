using CQRSlite.Commands;
using Nancy;
using Nancy.Security;
using neurUL.Cortex.Application.Neurons.Commands;
using neurUL.Cortex.Common;
using neurUL.Cortex.Domain.Model.Neurons;
using neurUL.Cortex.Port.Adapter.Common;
using System;

namespace neurUL.Cortex.Port.Adapter.In.Api
{
    public class TerminalModule : NancyModule
    {
        public TerminalModule(ICommandSender commandSender) : base("/cortex/terminals")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        false,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            TerminalModule.CreateTerminalFromDynamic(bodyAsObject, out Guid terminalId, out Guid presynapticNeuronId, 
                                out Guid postsynapticNeuronId, out NeurotransmitterEffect effect, out float strength, out Guid authorId);

                            return new CreateTerminal(terminalId, presynapticNeuronId, postsynapticNeuronId, 
                                effect, strength, authorId);
                        },
                        "Id",
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
                                Guid.Parse(parameters.terminalId),
                                Guid.Parse(bodyAsObject.AuthorId.ToString()),
                                expectedVersion
                                );
                        },
                        "AuthorId"
                    );
                }
            );
        }

        private static void CreateTerminalFromDynamic(dynamic dynamicTerminal, out Guid terminalId, out Guid presynapticNeuronId, 
            out Guid postsynapticNeuronId, out NeurotransmitterEffect effect, out float strength, out Guid authorId)
        {
            terminalId = Guid.Parse(dynamicTerminal.Id.ToString());
            presynapticNeuronId = Guid.Parse(dynamicTerminal.PresynapticNeuronId.ToString());
            postsynapticNeuronId = Guid.Parse(dynamicTerminal.PostsynapticNeuronId.ToString());
            string ne = dynamicTerminal.Effect.ToString();
            if (Enum.IsDefined(typeof(NeurotransmitterEffect), (int.TryParse(ne, out int ine) ? (object)ine : ne)))
                effect = (NeurotransmitterEffect)Enum.Parse(typeof(NeurotransmitterEffect), dynamicTerminal.Effect.ToString());
            else
                throw new ArgumentOutOfRangeException("Effect", $"Specified NeurotransmitterEffect value of '{dynamicTerminal.Effect.ToString()}' was invalid");
            strength = float.Parse(dynamicTerminal.Strength.ToString());
            authorId = Guid.Parse(dynamicTerminal.AuthorId.ToString());
        }
    }
}
