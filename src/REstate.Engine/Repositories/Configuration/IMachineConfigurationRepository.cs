using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface IMachineConfigurationRepository
    {
        Task<IEnumerable<MachineRecord>> ListMachinesAsync(CancellationToken cancellationToken);

        Task<Machine> RetrieveMachineConfigurationAsync(string machineName, CancellationToken cancellationToken);

        Task<Machine> RetrieveMachineConfigurationForInstanceAsync(string instanceId, CancellationToken cancellationToken);

        Task<Machine> DefineStateMachineAsync(Machine machine, string forkedFrom, CancellationToken cancellationToken);

        Task<Machine> DefineStateMachineAsync(Machine machine, CancellationToken cancellationToken);
    }
}
