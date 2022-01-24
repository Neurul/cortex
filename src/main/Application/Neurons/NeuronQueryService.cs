using CQRSlite.Domain;
using neurUL.Common.Domain.Model;
using neurUL.Cortex.Common;
using neurUL.Cortex.Domain.Model.Neurons;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace neurUL.Cortex.Application.Neurons
{
    public class NeuronQueryService : INeuronQueryService
    {
        private readonly ISession session;
        private readonly ISettingsService settingsService;

        public NeuronQueryService(ISession session, ISettingsService settingsService)
        {
            AssertionConcern.AssertArgumentNotNull(session, nameof(session));
            AssertionConcern.AssertArgumentNotNull(settingsService, nameof(settingsService));

            this.session = session;
            this.settingsService = settingsService;
        }

        public async Task<NeuronData> GetNeuronById(Guid id, CancellationToken token = default)
        {
            AssertionConcern.AssertArgumentValid(
                g => g != Guid.Empty,
                id,
                Messages.Exception.InvalidId,
                nameof(id)
                );

            var neuron = await this.session.Get<Neuron>(id, cancellationToken: token);

            return new NeuronData()
            {
                Id = neuron.Id.ToString(),
                Active = neuron.Active,
                Version = neuron.Version
            };
        }
    }
}
