using REstate.Client.Chrono.Models;
using System;
using System.Threading.Tasks;

namespace REstate.Client.Chrono
{
    public interface IChronoSession : IAuthenticatedSession
    {
        Task AddChronoTrigger(Guid machineInstanceId, string chronoTriggerJson);

        Task AddChronoTrigger(Guid machineInstanceId, string chronoTriggerJson, string payload);

        Task AddChronoTrigger(IChronoTriggerRequest chronoTrigger);
    }
}