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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Port.Adapter.In.Http
{
    public class NeuronModule : NancyModule
    {
        public NeuronModule(ICommandSender commandSender) : base("/cortex")
        {
            this.Put("/neurons/{neuronId}", async (parameters) =>
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
                                    ts.Add(new Terminal(Guid.Parse(bodyAsObject.Terminals[i].Target.ToString())));

                            var neuronId = Guid.Parse(parameters.neuronId);
                            string data = bodyAsObject.Data.ToString();
                            return ts.Count > 0 ?
                                (ICommand)new CreateNeuronWithTerminals(neuronId, data, ts) :
                                new CreateNeuron(neuronId, data);
                        },
                        "Data"
                    );
            }
            );

            this.Post("/neurons/{neuronId}/terminals", async (parameters) =>
            {
                return await NeuronModule.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            var ts = new List<Terminal>();
                            for (int i = 0; i < bodyAsObject.Terminals.Count; i++)
                                ts.Add(new Terminal(Guid.Parse(bodyAsObject.Terminals[i].Target.ToString())));
                            return new AddTerminalsToNeuron(Guid.Parse(parameters.neuronId), ts, expectedVersion);
                        },
                        "Terminals"
                    );
            }
            );

            this.Patch("/neurons/{neuronId}", async (parameters) =>
            {
                return await NeuronModule.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new ChangeNeuronData(Guid.Parse(parameters.neuronId), bodyAsObject.Data.ToString(), expectedVersion);
                        },
                        "Data"
                    );
            }
            );

            this.Delete("/neurons/{neuronId}/terminals/{terminalId}", async (parameters) =>
            {
                return await NeuronModule.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) => 
                        {
                            return new RemoveTerminalsFromNeuron(
                                Guid.Parse(parameters.neuronId),
                                new Terminal[] { new Terminal(Guid.Parse(parameters.terminalId)) },
                                expectedVersion
                                );
                        }
                    );
            }
            );

            this.Delete("/neurons/{neuronId}", async (parameters) =>
            {
                return await NeuronModule.ProcessCommandResponse(
                        commandSender,
                        this.Request,
                        (bodyAsObject, bodyAsDictionary, expectedVersion) =>
                        {
                            return new DeactivateNeuron(
                                Guid.Parse(parameters.neuronId), 
                                expectedVersion
                                );
                        }
                    );
            }
            );
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
            if (!string.IsNullOrEmpty(jsonString))
            {
                bodyAsObject = JsonConvert.DeserializeObject(jsonString);
                bodyAsDictionary = JObject.Parse(jsonString).ToObject<Dictionary<string, object>>();
            }

            var result = new Response { StatusCode = HttpStatusCode.OK };

            ICommand command = null;
            // validate required body fields
            string[] missingFieldsNames = requiredFields.Where(s => !bodyAsDictionary.ContainsKey(s)).ToArray();

            int expectedVersion = -1;
            if (versionRequired)
            {
                var rh = request.Headers["ETag"];
                if (requiredFields.Contains("ExpectedVersion") && !missingFieldsNames.Contains("ExpectedVersion"))
                    expectedVersion = int.Parse(bodyAsObject.ExpectedVersion.ToString());
                else if (!(rh.Any() && int.TryParse(rh.First(), out expectedVersion)))
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
