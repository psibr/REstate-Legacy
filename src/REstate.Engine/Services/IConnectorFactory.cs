using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Services
{
    public interface IConnectorFactory
    {
        string ConnectorKey { get; }

        Task<IConnector> BuildConnectorAsync(string apiKey, CancellationToken cancellationToken);

        bool IsActionConnector { get; }

        bool IsGuardConnector { get; }

        string ConnectorSchema { get; set; }
    }
}
