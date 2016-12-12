using System.Threading;
using System.Threading.Tasks;

namespace REstate.Scheduler
{
    public interface IChronoConsumer
    {
        Task StartAsync(string apiKey, CancellationToken cancellationToken);
    }
}
