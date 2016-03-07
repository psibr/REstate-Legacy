using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REstate.Client
{
    public interface IInstancesSession : IAuthenticatedSession
    {
        Task DeleteInstance(Guid machineInstanceGuid);
        Task<State> FireTrigger(Guid machineInstanceGuid, string triggerName, string payload = null);
        Task<ICollection<Trigger>> GetAvailableTriggers(Guid machineInstanceGuid);
        Task<string> GetMachineDiagram(Guid machineInstanceGuid);
        Task<State> GetMachineState(Guid machineInstanceGuid);
        Task<Guid> InstantiateMachine(int machineDefinitionId);
        Task<bool> IsMachineInState(Guid machineInstanceGuid, string stateName);
    }
}