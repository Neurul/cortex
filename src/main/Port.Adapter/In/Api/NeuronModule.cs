using CQRSlite.Commands;
using Nancy;
using Nancy.Security;
using org.neurul.Common.Domain.Model;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Port.Adapter.Common;
using System;
using System.Linq;

namespace org.neurul.Cortex.Port.Adapter.In.Api
{
    public class NeuronModule : NancyModule
    {
        public NeuronModule(ICommandSender commandSender) : base("/{avatarId}/cortex/neurons")
        {
            // TODO: transfer to sentry
            // if (bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.RequireAuthentication), out bool value) && value)
            //    this.RequiresAuthentication();

            this.Post(string.Empty, async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        false,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new CreateNeuron(
                                parameters.avatarId,
                                Guid.Parse(bodyAsObject.Id.ToString()),
                                Guid.Parse(bodyAsObject.AuthorId.ToString())
                                );                            
                        },
                        "Id",
                        "AuthorId"
                    );
            }
            );

            this.Delete("/{neuronId}", async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new DeactivateNeuron(
                                parameters.avatarId,
                                Guid.Parse(parameters.neuronId),
                                Guid.Parse(bodyAsObject.AuthorId.ToString()),
                                expectedVersion
                                );
                        },
                        "AuthorId"
                    );
            }
            );
        }
    }
}
