using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface IMachineInstancesRepository
    {
        Task CreateInstanceAsync(string machineName, string instanceId, IDictionary<string, string> metadata, CancellationToken cancellationToken);

        Task CreateInstanceAsync(string machineName, string instanceId, CancellationToken cancellationToken);

        Task DeleteInstanceAsync(string instanceId, CancellationToken cancellationToken);

        Task<InstanceRecord> GetInstanceStateAsync(string instanceId, CancellationToken cancellationToken);

        Task<InstanceRecord> SetInstanceStateAsync(string instanceId, string stateName, string triggerName, string lastCommitTag, CancellationToken cancellationToken);

        Task<InstanceRecord> SetInstanceStateAsync(string instanceId, string stateName, string triggerName, string parameterData, string lastCommitTag, CancellationToken cancellationToken);

        Task<string> GetInstanceMetadataAsync(string instanceId, CancellationToken cancellationToken);
    }
}
