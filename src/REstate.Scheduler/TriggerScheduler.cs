using REstate.Scheduling;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Scheduler
{
    public class TriggerScheduler
    {
        private readonly IChronoRepositoryFactory _ChronoRepositoryFactory;
        private readonly StringSerializer _StringSerializer;

        private readonly string _ApiKey;

        public TriggerScheduler(IChronoRepositoryFactory chronoRepositoryfactory, StringSerializer stringSerializer, string apiKey)
        {
            _ChronoRepositoryFactory = chronoRepositoryfactory;

            _StringSerializer = stringSerializer;

            _ApiKey = apiKey;
        }

        public async Task ScheduleTrigger(IChronoTrigger trigger, CancellationToken cancellationToken)
        {
            using (var repository = _ChronoRepositoryFactory.OpenRepository(_ApiKey))
            {
                await repository.AddChronoTrigger(trigger, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task RemoveTrigger(IChronoTrigger trigger, CancellationToken cancellationToken)
        {
            using (var repository = _ChronoRepositoryFactory.OpenRepository(_ApiKey))
            {
                await repository.RemoveChronoTrigger(trigger, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
