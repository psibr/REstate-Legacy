using System;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Client
{
    public interface IConfigurationSession : IAuthenticatedSession
    {
        Task<IStateMachineConfiguration> DefineStateMachine(IStateMachineConfiguration configuration);
        Task<string> GetMachineDiagram(int machineDefinitionId);
        Task<IStateMachineConfiguration> GetStateMachineConfiguration(int machineDefinitionId);
    }
}