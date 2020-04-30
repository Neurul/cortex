using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using neurUL.Cortex.Application.Neurons;

namespace neurUL.Cortex.Port.Adapter.Out.Api
{
    public class NeuronModule : NancyModule
    {
        public NeuronModule(INeuronQueryService neuronQueryService) : base("/cortex/neurons")
        {
            // use this from clients instead of neurongraphclient since latter is eventually consistent
            // returned version can be used by client to check if change is applicable
            this.Get("/{neuronId}", async (parameters) => new TextResponse(JsonConvert.SerializeObject(
                await neuronQueryService.GetNeuronById(parameters.neuronId))
                )
                );
        }
    }
}