namespace REstate.Scheduler
{
    public class TriggerSchedulerFactory
    {
        private readonly IChronoRepositoryFactory _RepositoryFactory;

        private readonly StringSerializer _StringSerializer;

        public TriggerSchedulerFactory(
            IChronoRepositoryFactory repositoryFactory,
            StringSerializer stringSerializer)
        {
            _RepositoryFactory = repositoryFactory;

            _StringSerializer = stringSerializer;
        }

        public TriggerScheduler GetTriggerScheduler(string apiKey)
        {
            return new TriggerScheduler(
                _RepositoryFactory,
                _StringSerializer,
                apiKey);
        }
    }
}
