using System.Threading.Tasks;

namespace REstate.Services
{
    public interface IConnectorFactory
    {
        string ConnectorKey { get; }

        Task<IConnector> BuildConnector(string apiKey);
    }
}