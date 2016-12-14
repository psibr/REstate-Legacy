using REstate;
using REstate.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace REstateClient
{
    public interface IInstancesSession : IAuthenticatedSession
    {
        Task DeleteInstanceAsync(string instanceId, CancellationToken cancellationToken);

        Task<State> FireTriggerAsync(string instanceId, string triggerName, string contentType, string payload, string commitTag, CancellationToken cancellationToken);

        Task<ICollection<Trigger>> GetAvailableTriggersAsync(string instanceId, CancellationToken cancellationToken);

        Task<string> GetDiagramAsync(string instanceId, CancellationToken cancellationToken);

        Task<State> GetStateAsync(string instanceId, CancellationToken cancellationToken);

        Task<InstanceRecord> GetInstanceInfoAsync(string instanceId, CancellationToken cancellationToken);

        Task<string> InstantiateAsync(string machineName, CancellationToken cancellationToken);

        Task<bool> IsInStateAsync(string instanceId, string stateName, CancellationToken cancellationToken);
    }
}
