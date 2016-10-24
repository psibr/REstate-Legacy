using REstateClient.Models;
using System.Threading.Tasks;

namespace REstateClient
{
    public interface IChronoSession : IAuthenticatedSession
    {
        Task AddChronoTrigger(string machineInstanceId, string chronoTriggerJson);

        Task AddChronoTrigger(string machineInstanceId, string chronoTriggerJson, string payload);

        Task AddChronoTrigger(IChronoTriggerRequest chronoTrigger);
    }
}
