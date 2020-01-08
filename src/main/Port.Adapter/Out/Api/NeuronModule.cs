using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using org.neurul.Cortex.Application.Neurons;

namespace org.neurul.Cortex.Port.Adapter.Out.Api
{
    public class NeuronModule : NancyModule
    {
        public NeuronModule(INeuronQueryService neuronQueryService) : base("/{avatarId}/cortex/neurons")
        {
            this.Get("/{neuronId}", async (parameters) => new TextResponse(JsonConvert.SerializeObject(
                await neuronQueryService.GetNeuronById(parameters.avatarId, parameters.neuronId))
                )
                );
        }
    }
}