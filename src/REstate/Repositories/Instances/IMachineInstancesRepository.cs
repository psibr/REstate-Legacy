using REstate.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Repositories.Instances
{
    public interface IMachineInstancesRepository
    { 

        Task EnsureInstanceExists(IStateMachineConfiguration configuration, string machineInstanceId, CancellationToken cancellationToken);

        Task DeleteInstance(string machineInstanceId, CancellationToken cancellationToken);

        State GetInstanceState(string machineInstanceId);

        void SetInstanceState(string machineInstanceId, State state, State lastState);
    }
}