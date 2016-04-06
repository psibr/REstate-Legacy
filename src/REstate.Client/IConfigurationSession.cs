using REstate.Configuration;
using System.Threading.Tasks;

namespace REstate.Client
{
    public interface IConfigurationSession : IAuthenticatedSession
    {
        Task<IStateMachineConfiguration> DefineStateMachine(IStateMachineConfiguration configuration);
        Task<string> GetMachineDiagram(string machineDefinitionId);
        Task<IStateMachineConfiguration> GetStateMachineConfiguration(string machineDefinitionId);
    }
}