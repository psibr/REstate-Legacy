using REstate;
using REstate.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace REstateClient
{
    public interface IInstancesSession : IAuthenticatedSession
    {
        Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken);

        Task<State> FireTriggerAsync(string machineId, string triggerName, string contentType, string payload, Guid? commitTag, CancellationToken cancellationToken);

        Task<ICollection<Trigger>> GetAvailableTriggersAsync(string machineId, CancellationToken cancellationToken);

        Task<string> GetMachineDiagramAsync(string machineId, CancellationToken cancellationToken);

        Task<State> GetStateAsync(string machineId, CancellationToken cancellationToken);

        Task<InstanceRecord> GetInstanceInfoAsync(string machineId, CancellationToken cancellationToken);

        Task<string> InstantiateAsync(string schematicName, CancellationToken cancellationToken);

        Task<bool> IsInStateAsync(string machineId, string stateName, CancellationToken cancellationToken);
    }
}
