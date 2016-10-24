using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface IMachineConfigurationRepository
    {
        Task<IEnumerable<MachineRecord>> ListMachines(CancellationToken cancellationToken);

        Task<Machine> RetrieveMachineConfiguration(string machineName, CancellationToken cancellationToken);

        Task<Machine> RetrieveMachineConfigurationForInstance(string instanceId, CancellationToken cancellationToken);

        Task<Machine> DefineStateMachine(Machine machine, string forkedFrom, CancellationToken cancellationToken);

        Task<Machine> DefineStateMachine(Machine machine, CancellationToken cancellationToken);
    }
}
