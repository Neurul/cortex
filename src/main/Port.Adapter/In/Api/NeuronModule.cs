using CQRSlite.Commands;
using CQRSlite.Domain.Exception;
using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Nancy.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using org.neurul.Cortex.Application.Neurons.Commands;
using org.neurul.Cortex.Domain.Model.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Port.Adapter.In.Api
{
    public class NeuronModule : NancyModule
    {
        public NeuronModule(ICommandSender commandSender) : base("/{avatarId}/cortex/neurons")
        {
            this.Put("/{neuronId}", async (parameters) =>
            {
                return await NeuronModule.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        false,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            // process optional fields
                            var ts = new List<Terminal>();
                            if (bodyAsDictionary.ContainsKey("Terminals"))
                                for (int i = 0; i < bodyAsObject.Terminals.Count; i++)
                                    ts.Add((Terminal)NeuronModule.CreateTerminalFromDynamic(bodyAsObject.Terminals[i]));

                            var neuronId = System.Guid.Parse(parameters.neuronId);
                            var avatarId = parameters.avatarId;
                            string data = bodyAsObject.Data.ToString();
                            string authorId = bodyAsObject.AuthorId.ToString();
                            return ts.Count > 0 ?
                                (ICommand)new CreateNeuronWithTerminals(avatarId, neuronId, data, ts, authorId) :
                                new CreateNeuron(avatarId, neuronId, data, authorId);
                        },
                        "Data",
                        "AuthorId"
                    );
            }
            );

            this.Post("/{neuronId}/terminals", async (parameters) =>
            {
                return await NeuronModule.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            var ts = new List<Terminal>();
                            for (int i = 0; i < bodyAsObject.Terminals.Count; i++)
                                ts.Add(NeuronModule.CreateTerminalFromDynamic(bodyAsObject.Terminals[i]));
                            return new AddTerminalsToNeuron(
                                parameters.avatarId, 
                                Guid.Parse(parameters.neuronId), 
                                ts,
                                bodyAsObject.AuthorId.ToString(), 
                                expectedVersion
                                );
                        },
                        "Terminals",
                        "AuthorId"
                    );
            }
            );

            this.Patch("/{neuronId}", async (parameters) =>
            {
                return await NeuronModule.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new ChangeNeuronData(
                                parameters.avatarId, 
                                Guid.Parse(parameters.neuronId), 
                                bodyAsObject.Data.ToString(),
                                bodyAsObject.AuthorId.ToString(), 
                                expectedVersion
                                );
                        },
                        "Data",
                        "AuthorId"
                    );
            }
            );

            this.Delete("/{neuronId}/terminals/{targetId}", async (parameters) =>
            {
                return await NeuronModule.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) => 
                        {
                            return new RemoveTerminalsFromNeuron(
                                parameters.avatarId,
                                Guid.Parse(parameters.neuronId),
                                new string[] { parameters.targetId.ToString() },
                                bodyAsObject.AuthorId.ToString(),
                                expectedVersion
                                );
                        },
                        "AuthorId"
                    );
            }
            );

            this.Delete("/{neuronId}", async (parameters) =>
            {
                return await NeuronModule.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new DeactivateNeuron(
                                parameters.avatarId,
                                Guid.Parse(parameters.neuronId),
                                bodyAsObject.AuthorId.ToString(),
                                expectedVersion
                                );
                        },
                        "AuthorId"
                    );
            }
            );
        }

        private static Terminal CreateTerminalFromDynamic(dynamic dynamicTerminal)
        {
            string ne = dynamicTerminal.Effect.ToString();
            if (Enum.IsDefined(typeof(NeurotransmitterEffect), (int.TryParse(ne, out int ine) ? (object) ine : ne)))
            {
                return new Terminal(
                    Guid.Parse(dynamicTerminal.TargetId.ToString()),
                    (NeurotransmitterEffect)Enum.Parse(typeof(NeurotransmitterEffect), dynamicTerminal.Effect.ToString()),
                    float.Parse(dynamicTerminal.Strength.ToString())
                    );
            }
            else
                throw new ArgumentOutOfRangeException("Effect", $"Specified NeurotransmitterEffect value of '{dynamicTerminal.Effect.ToString()}' was invalid");
        }

        private static async Task<Response> ProcessCommandResponse(ICommandSender commandSender, Request request, Func<dynamic, Dictionary<string, object>, int, ICommand> commandGenerator, params string[] requiredFields)
        {
            return await NeuronModule.ProcessCommandResponse(commandSender, request, true, commandGenerator, requiredFields);
        }

        private static async Task<Response> ProcessCommandResponse(ICommandSender commandSender, Request request, bool versionRequired, Func<dynamic, Dictionary<string, object>, int, ICommand> commandGenerator, params string[] requiredFields)
        {
            if (commandSender == null)
                throw new ArgumentNullException(nameof(commandSender));
            if (commandGenerator == null)
                throw new ArgumentNullException(nameof(commandGenerator));

            dynamic bodyAsObject = null;
            Dictionary<string, object> bodyAsDictionary = null;

            var jsonString = RequestStream.FromStream(request.Body).AsString();

            string[] missingFieldsNames = null;

            if (!string.IsNullOrEmpty(jsonString))
            {
                bodyAsObject = JsonConvert.DeserializeObject(jsonString);
                bodyAsDictionary = JObject.Parse(jsonString).ToObject<Dictionary<string, object>>();
                missingFieldsNames = requiredFields.Where(s => !bodyAsDictionary.ContainsKey(s)).ToArray();
            }
            else
                missingFieldsNames = requiredFields;

            var result = new Response { StatusCode = HttpStatusCode.OK };

            ICommand command = null;
            // validate required body fields

            int expectedVersion = -1;
            if (versionRequired)
            {
                var rh = request.Headers["ETag"];
                if (!(rh.Any() && int.TryParse(rh.First(), out expectedVersion)))
                    missingFieldsNames = missingFieldsNames.Concat(new string[] { "ExpectedVersion" }).ToArray();
            }

            if (missingFieldsNames.Count() == 0)
                command = commandGenerator.Invoke(bodyAsObject, bodyAsDictionary, expectedVersion);
            else
                result = new TextResponse(
                    HttpStatusCode.BadRequest,
                    $"Required field(s) '{ string.Join("', '", missingFieldsNames) }' not found."
                    );

            if (result.StatusCode != HttpStatusCode.BadRequest)
            {
                try
                {
                    await commandSender.Send(command);
                }
                catch (Exception ex)
                {
                    HttpStatusCode hsc = HttpStatusCode.BadRequest;

                    if (ex is ConcurrencyException)
                        hsc = HttpStatusCode.Conflict;

                    result = new TextResponse(hsc, ex.ToString());
                }
            }

            return result;
        } 
    }
}
