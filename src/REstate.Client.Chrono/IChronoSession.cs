using System;
using System.Threading.Tasks;
using REstate.Chrono;

namespace REstate.Client.Chrono
{
    public interface IChronoSession : IAuthenticatedSession
    {
        Task AddChronoTrigger(Guid machineInstanceId, string chronoTriggerJson);

        Task AddChronoTrigger(Guid machineInstanceId, string chronoTriggerJson, string payload);

        Task AddChronoTrigger(IChronoTrigger chronoTrigger);
    }
}