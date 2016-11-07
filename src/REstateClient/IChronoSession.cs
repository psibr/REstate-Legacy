using REstate.Scheduling;
using System.Threading.Tasks;

namespace REstateClient
{
    public interface IChronoSession : IAuthenticatedSession
    {
        Task AddChronoTrigger(ChronoTrigger chronoTrigger);
    }
}
