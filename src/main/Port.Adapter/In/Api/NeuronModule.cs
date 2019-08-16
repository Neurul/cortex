using CQRSlite.Commands;
using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Nancy.Responses;
using Nancy.Security;
using Newtonsoft.Json;
using org.neurul.Cortex.Application.IdentityAccess;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Port.Adapter.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Port.Adapter.In.Api
{
    public class NeuronModule : NancyModule
    {
        private const string IdentityPermissionKey = "IdentityPermission";

        public NeuronModule(ICommandSender commandSender, IIdentityPermissionApplicationService identityPermissionApplicationService) : base("/{avatarId}/cortex/neurons")
        {
            if (bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentVariableKeys.RequireAuthentication), out bool value) && value)
                this.RequiresAuthentication();

            this.Before.AddItemToStartOfPipeline(async(ctx, token) =>
            {
                Response result = null;
                try
                {
                    IdentityPermissionData ip = await identityPermissionApplicationService.GetBySubjectIdAvatar(
                        ctx.CurrentUser.Claims.First(c => c.Type == "sub").Value,
                        ctx.Parameters.avatarId
                        );

                    dynamic body = JsonConvert.DeserializeObject(
                        RequestStream.FromStream(ctx.Request.Body).AsString()
                        );

                    var requestedLayer = body.AuthorId.ToString();
                    var layerAccess = ip.AccessibleLayers.SingleOrDefault(l => l.NeuronId == requestedLayer);
                    if (ip == null)
                        result = new TextResponse(HttpStatusCode.Unauthorized, "User not authorized to read Avatar.");
                    else if (layerAccess == null)
                        result = new TextResponse(HttpStatusCode.Unauthorized, "User not authorized to access Layer.");
                    else if (layerAccess != null && !layerAccess.CanWrite)
                        result = new TextResponse(HttpStatusCode.Unauthorized, "User not unauthorized to write to Layer.");
                    else
                        ctx.Items.Add(NeuronModule.IdentityPermissionKey, ip);                    
                }
                catch (Exception ex)
                {
                    result = new TextResponse(HttpStatusCode.BadRequest, ex.ToString());
                }

                return result;
            });

            this.Put("/{neuronId}", async (parameters) =>
            {
                // TODO: make idempotent since HTTP PUT should allow any number of calls or change to POST?
                return await Helper.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        false,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            var neuronId = Guid.Parse(parameters.neuronId);
                            var avatarId = parameters.avatarId;
                            string tag = bodyAsObject.Tag.ToString();
                            var authorId = ((IdentityPermissionData) this.Context.Items[NeuronModule.IdentityPermissionKey]).NeuronId;
                            return new CreateNeuron(avatarId, neuronId, tag, authorId);
                        },
                        "Tag",
                        "AuthorId"
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
                                ((IdentityPermissionData)this.Context.Items[NeuronModule.IdentityPermissionKey]).NeuronId, 
                                expectedVersion
                                );
                        },
                        "Tag",
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
                                ((IdentityPermissionData)this.Context.Items[NeuronModule.IdentityPermissionKey]).NeuronId,
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
