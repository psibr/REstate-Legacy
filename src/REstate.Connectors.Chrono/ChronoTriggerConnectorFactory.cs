using REstate.Client;
using REstate.Client.Chrono;
using REstate.Services;
using System.Threading.Tasks;
using REstate.Logging;

namespace REstate.Connectors.Chrono
{
    public class ChronoTriggerConnectorFactory
        : IConnectorFactory
    {
        private readonly IAuthSessionClient<IChronoSession> _client;
        private readonly IREstateLogger _logger;

        public ChronoTriggerConnectorFactory(IAuthSessionClient<IChronoSession> client, IREstateLogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public string ConnectorKey { get; } = "REstate.Connectors.Chrono";

        public async Task<IConnector> BuildConnector(string apiKey) => 
            new ChronoTriggerConnector(await _client.GetSession(apiKey));
    }
}