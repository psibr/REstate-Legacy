using REstate.Scheduling;
using System.Threading.Tasks;
using System.Threading;

namespace REstateClient
{
    public interface IChronoSession : IAuthenticatedSession
    {
        Task AddChronoTriggerAsync(ChronoTrigger chronoTrigger, CancellationToken cancellationToken);
    }
}
