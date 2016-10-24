using System.Threading.Tasks;

namespace REstate.Scheduler
{
    public interface IChronoConsumer
    {
        void Start(string apiKey);

        Task StartAsync(string apiKey);

        void Stop();
    }
}
