using System;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Repositories.Instances;

namespace REstate.Repositories.Core.Consul
{
    public class MachineInstancesRepository
        : CoreContextualRepository, IMachineInstancesRepository
    {

        public MachineInstancesRepository(CoreRepository parent)
            : base(parent)
        {
        }

        public Task EnsureInstanceExists(IStateMachineConfiguration configuration, string machineInstanceId,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteInstance(string machineInstanceId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public State GetInstanceState(string machineInstanceId)
        {
            throw new NotImplementedException();
        }

        public void SetInstanceState(string machineInstanceId, State state, State lastState)
        {
            throw new NotImplementedException();
        }
    }
}