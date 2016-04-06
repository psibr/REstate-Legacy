using REstate.Client.Chrono.Models;
using System.Threading.Tasks;

namespace REstate.Client.Chrono
{
    public interface IChronoSession : IAuthenticatedSession
    {
        Task AddChronoTrigger(string machineInstanceId, string chronoTriggerJson);

        Task AddChronoTrigger(string machineInstanceId, string chronoTriggerJson, string payload);

        Task AddChronoTrigger(IChronoTriggerRequest chronoTrigger);
    }
}