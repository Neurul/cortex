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
            if (bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.RequireAuthentication), out bool value) && value)
                this.RequiresAuthentication();

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
                                bodyAsObject.Tag.ToString(),
                                Guid.Parse(bodyAsObject.LayerId.ToString()),
                                NeuronModule.GetUserSubjectId(this.Context)
                                );                            
                        },
                        "Id",
                        "Tag",
                        "LayerId"
                    );
            }
            );

            this.Patch("/{neuronId}", async (parameters) =>
            {
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new ChangeNeuronTag(
                                parameters.avatarId,
                                Guid.Parse(parameters.neuronId),
                                bodyAsObject.Tag.ToString(),
                                NeuronModule.GetUserSubjectId(this.Context),
                                expectedVersion
                                );
                        },
                        "Tag"
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
                                NeuronModule.GetUserSubjectId(this.Context),
                                expectedVersion
                                );
                        }
                    );
            }
            );
        }

        internal static Guid GetUserSubjectId(NancyContext context)
        {
            Guid result = Guid.Empty;

            if (bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.RequireAuthentication), out bool value) && value)
            {
                AssertionConcern.AssertArgumentValid(c => c.CurrentUser != null, context, "Context User is null or not found.", nameof(context));
                result = Guid.Parse(context.CurrentUser.Claims.First(c => c.Type == "sub").Value);
            }
            else
                result = Guid.Parse(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TestUserSubjectId));

            return result;
        }
    }
}
