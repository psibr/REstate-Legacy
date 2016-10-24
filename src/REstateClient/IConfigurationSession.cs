using REstate.Configuration;
using System.Threading.Tasks;

namespace REstateClient
{
    public interface IConfigurationSession : IAuthenticatedSession, IInstancesSession
    {
        Task<Machine> DefineStateMachine(Machine configuration);

        Task<string> GetMachineDiagram(string machineName);

        Task<Machine> GetMachine(string machineName);
    }
}
