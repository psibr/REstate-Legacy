using REstate.Scheduling;
using REstateClient.Models;
using System.Threading.Tasks;

namespace REstateClient
{
    public interface IChronoSession : IAuthenticatedSession
    {
        Task AddChronoTrigger(ChronoTrigger chronoTrigger);
    }
}
