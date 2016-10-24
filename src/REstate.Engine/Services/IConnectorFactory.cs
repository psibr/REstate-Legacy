using System.Threading.Tasks;

namespace REstate.Engine.Services
{
    public interface IConnectorFactory
    {
        string ConnectorKey { get; }

        Task<IConnector> BuildConnector(string apiKey);

        bool IsActionConnector { get; }

        bool IsGuardConnector { get; }

        string ConnectorSchema { get; set; }
    }
}
