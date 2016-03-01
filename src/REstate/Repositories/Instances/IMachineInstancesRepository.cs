using System;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Repositories.Instances
{
    public interface IMachineInstancesRepository
    { 

        Task EnsureInstanceExists(IStateMachineConfiguration configuration, Guid machineInstanceGuid, CancellationToken cancellationToken);

        Task DeleteInstance(Guid machineInstanceGuid, CancellationToken cancellationToken);

        State GetInstanceState(Guid machineInstanceGuid);

        void SetInstanceState(Guid machineInstanceGuid, State state, State lastState);
    }
}