using REstate.Services;
using System.Threading.Tasks;

namespace REstate.Connectors.Susanoo
{
    public class SusanooConnectorFactory
        : IConnectorFactory
    {

        public string ConnectorKey { get; } = "REstate.Connectors.Susanoo";

        public Task<IConnector> BuildConnector(string apiKey) => 
            Task.FromResult<IConnector>(new SusanooConnector(apiKey));
    }
}