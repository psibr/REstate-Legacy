using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace REstate.Client
{
    public interface IInstancesSession : IAuthenticatedSession
    {
        Task DeleteInstance(string machineInstanceId);
        Task<State> FireTrigger(string machineInstanceId, string triggerName, string payload = null);
        Task<ICollection<Trigger>> GetAvailableTriggers(string machineInstanceId);
        Task<string> GetMachineDiagram(string machineInstanceId);
        Task<State> GetMachineState(string machineInstanceId);
        Task<Guid> InstantiateMachine(string machineDefinitionId);
        Task<bool> IsMachineInState(string machineInstanceId, string stateName);
    }
}