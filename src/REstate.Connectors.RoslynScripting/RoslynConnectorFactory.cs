using REstate.Services;
using System.Threading.Tasks;
using Psibr.Platform.Logging;

namespace REstate.Connectors.RoslynScripting
{
    public class RoslynConnectorFactory
        : IConnectorFactory
    {
        private readonly IPlatformLogger _logger;

        public RoslynConnectorFactory(IPlatformLogger logger)
        {
            _logger = logger;
        }

        public string ConnectorKey { get; } = "REstate.Connectors.RoslynScripting";

        public Task<IConnector> BuildConnector(string apiKey)
        {
            return Task.FromResult<IConnector>(new RoslynConnector(_logger, apiKey));
        }
    }
}