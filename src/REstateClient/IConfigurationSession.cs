using REstate.Configuration;
using System.Threading.Tasks;
using System.Threading;

namespace REstateClient
{
    public interface IConfigurationSession : IAuthenticatedSession, IInstancesSession
    {
        Task<Machine> DefineStateMachineAsync(Machine configuration, CancellationToken cancellationToken);

        Task<string> GetMachineDiagramAsync(string machineName, CancellationToken cancellationToken);

        Task<Machine> GetMachineAsync(string machineName, CancellationToken cancellationToken);
    }
}
