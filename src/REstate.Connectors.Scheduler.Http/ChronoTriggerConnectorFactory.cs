using REstate.Engine.Services;
using REstate.Logging;
using REstateClient;
using System.Threading.Tasks;
using System.Threading;

namespace REstate.Engine.Connectors.Scheduler
{
    public class ChronoTriggerConnectorFactory
        : IConnectorFactory
    {
        private readonly REstateChronoClient _Client;
        private readonly IPlatformLogger _logger;

        public ChronoTriggerConnectorFactory(REstateChronoClient client, IPlatformLogger logger)
        {
            _Client = client;
            _logger = logger;
        }

        public string ConnectorKey => ChronoTriggerConnector.ConnectorKey;
        public bool IsActionConnector { get; } = true;
        public bool IsGuardConnector { get; } = false;
        public string ConnectorSchema { get; set; } = "{ }";

        public async Task<IConnector> BuildConnectorAsync(string apiKey, CancellationToken cancellationToken) =>
            new ChronoTriggerConnector(await _Client.GetSessionAsync(apiKey, cancellationToken).ConfigureAwait(false));
    }
}
