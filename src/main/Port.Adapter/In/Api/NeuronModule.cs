using CQRSlite.Domain.Exception;
using Nancy;
using neurUL.Common.Api;
using neurUL.Cortex.Application.Neurons.Commands;
using neurUL.Cortex.Port.Adapter.In.InProcess;
using System;

namespace neurUL.Cortex.Port.Adapter.In.Api
{
    public class NeuronModule : NancyModule
    {
        public NeuronModule(INeuronAdapter neuronAdapter) : base("/cortex/neurons")
        {
            this.Post(string.Empty, async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        false,
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                            await neuronAdapter.CreateNeuron(
                                Guid.Parse(bodyAsObject.Id.ToString()),
                                Guid.Parse(bodyAsObject.AuthorId.ToString())
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
                        "Id",
                        "AuthorId"
                    );
            }
            );

            this.Delete("/{neuronId}", async (parameters) =>
            {
                return await this.Request.ProcessCommand(
                        async (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                            await neuronAdapter.DeactivateNeuron(
                                Guid.Parse(parameters.neuronId),
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
    }
}
