using CQRSlite.Commands;
using CQRSlite.Domain.Exception;
using Nancy;
using neurUL.Common.Api;
using neurUL.Cortex.Application.Neurons.Commands;
using neurUL.Cortex.Common;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;

namespace neurUL.Cortex.Port.Adapter.In.Api
{
    public class TerminalModule : NancyModule
    {
        public TerminalModule(ITerminalAdapter terminalAdapter) : base("/cortex/terminals")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        false,
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            TerminalModule.CreateTerminalFromDynamic(bodyAsObject, out Guid terminalId, out Guid presynapticNeuronId, 
                                out Guid postsynapticNeuronId, out NeurotransmitterEffect effect, out float strength, out Guid authorId);

                            await terminalAdapter.CreateTerminal(terminalId, presynapticNeuronId, postsynapticNeuronId, 
                                effect, strength, authorId);
                        },
                        (ex, hsc) => {
                            // TODO: immediately cause calling Polly to fail (handle specific failure http code to signal "it's not worth retrying"?)
                            // i.e. there is an issue with the data
                            HttpStatusCode result = hsc;
                            if (ex is ConcurrencyException)
                                result = HttpStatusCode.Conflict;
                            return result;
                        },
                        Array.Empty<string>(),
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
                return await this.Request.ProcessCommand(
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                            await terminalAdapter.DeactivateTerminal(
                                Guid.Parse(parameters.terminalId),
                                Guid.Parse(bodyAsObject.AuthorId.ToString()),
                                expectedVersion
                                ),
                        (ex, hsc) => {
                            // TODO: immediately cause calling Polly to fail (handle specific failure http code to signal "it's not worth retrying"?)
                            // i.e. there is an issue with the data
                            HttpStatusCode result = hsc;
                            if (ex is ConcurrencyException)
                                result = HttpStatusCode.Conflict;
                            return result;
                        },
                        Array.Empty<string>(),
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
