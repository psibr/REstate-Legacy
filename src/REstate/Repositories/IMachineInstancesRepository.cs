using System;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Repositories
{
    public interface IMachineInstancesRepository
    { 

        Task EnsureInstanceExists(IStateMachineConfiguration configuration, Guid machineInstanceGuid, CancellationToken cancellationToken);

        Task DeleteInstance(Guid machineInstanceGuid, CancellationToken cancellationToken);

        Task<State> GetInstanceState(Guid machineInstanceGuid, CancellationToken cancellationToken);

        Task SetInstanceState(Guid machineInstanceGuid, State state, CancellationToken cancellationToken);
    }
}