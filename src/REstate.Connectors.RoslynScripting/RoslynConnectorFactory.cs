using REstate.Services;
using System.Threading.Tasks;

namespace REstate.Connectors.RoslynScripting
{
    public class RoslynConnectorFactory
        : IConnectorFactory
    {
        public string ConnectorKey { get; } = "REstate.Connectors.RoslynScripting";

        public Task<IConnector> BuildConnector(string apiKey)
        {
            return Task.FromResult<IConnector>(new RoslynConnector(apiKey));
        }
    }
}