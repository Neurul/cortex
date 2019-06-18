using CQRSlite.Commands;
using CQRSlite.Domain.Exception;
using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Nancy.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Port.Adapter.In.Api
{
    public static class Helper
    {
        internal static async Task<Response> ProcessCommandResponse(ICommandSender commandSender, Request request, Func<dynamic, Dictionary<string, object>, int, ICommand> commandGenerator, params string[] requiredFields)
        {
            return await Helper.ProcessCommandResponse(commandSender, request, true, commandGenerator, requiredFields);
        }

        internal static async Task<Response> ProcessCommandResponse(ICommandSender commandSender, Request request, bool versionRequired, Func<dynamic, Dictionary<string, object>, int, ICommand> commandGenerator, params string[] requiredFields)
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
