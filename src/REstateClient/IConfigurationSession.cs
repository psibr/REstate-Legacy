using REstate.Configuration;
using System.Threading.Tasks;
using System.Threading;

namespace REstateClient
{
    public interface IConfigurationSession : IAuthenticatedSession, IInstancesSession
    {
        Task<Schematic> CreateSchematicAsync(Schematic configuration, CancellationToken cancellationToken);

        Task<string> GetSchematicDiagramAsync(string schematicName, CancellationToken cancellationToken);

        Task<Schematic> GetSchematicAsync(string schematicName, CancellationToken cancellationToken);
    }
}
