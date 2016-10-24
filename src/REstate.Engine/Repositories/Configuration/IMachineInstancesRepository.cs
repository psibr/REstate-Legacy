using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface IMachineInstancesRepository
    {
        Task CreateInstance(string machineName, string instanceId, IDictionary<string, string> metadata, CancellationToken cancellationToken);

        Task CreateInstance(string machineName, string instanceId, CancellationToken cancellationToken);

        Task DeleteInstance(string instanceId, CancellationToken cancellationToken);

        Task<InstanceRecord> GetInstanceState(string instanceId, CancellationToken cancellationToken);

        Task SetInstanceState(string instanceId, string stateName, string triggerName, string lastCommitTag, CancellationToken cancellationToken);

        Task SetInstanceState(string instanceId, string stateName, string triggerName, string parameterData, string lastCommitTag, CancellationToken cancellationToken);

        Task<string> GetInstanceMetadata(string instanceId, CancellationToken cancellationToken);
    }
}
