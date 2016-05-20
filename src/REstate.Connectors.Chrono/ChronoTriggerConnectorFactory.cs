using REstate.Client;
using REstate.Client.Chrono;
using REstate.Services;
using System.Threading.Tasks;
using Psibr.Platform.Logging;

namespace REstate.Connectors.Chrono
{
    public class ChronoTriggerConnectorFactory
        : IConnectorFactory
    {
        private readonly IAuthSessionClient<IChronoSession> _client;
        private readonly IPlatformLogger _logger;

        public ChronoTriggerConnectorFactory(IAuthSessionClient<IChronoSession> client, IPlatformLogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public string ConnectorKey { get; } = "REstate.Connectors.Chrono";
        public bool IsActionConnector { get; } = true;
        public bool IsGuardConnector { get; } = false;
        public string ConnectorSchema { get; set; } = "{ }";

        public async Task<IConnector> BuildConnector(string apiKey) => 
            new ChronoTriggerConnector(await _client.GetSession(apiKey));
    }
}