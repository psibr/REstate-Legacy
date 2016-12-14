using REstate.Engine.Services;
using REstate.Logging;
using REstate.Scheduler;
using System.Threading.Tasks;
using System.Threading;

namespace REstate.Engine.Connectors.Scheduler
{
    public class DirectChronoTriggerConnectorFactory
        : IConnectorFactory
    {
        private readonly TriggerSchedulerFactory _TriggerSchedulerFactory;

        private readonly StringSerializer _StringSerializer;
        private readonly IPlatformLogger _logger;

        public DirectChronoTriggerConnectorFactory(TriggerSchedulerFactory triggerSchedulerFactory, StringSerializer stringSerializer, IPlatformLogger logger)
        {
            _TriggerSchedulerFactory = triggerSchedulerFactory;

            _StringSerializer = stringSerializer;
            _logger = logger;
        }

        public string ConnectorKey => DirectChronoTriggerConnector.ConnectorKey;
        public bool IsActionConnector { get; } = true;
        public bool IsGuardConnector { get; } = false;
        public string ConnectorSchema { get; set; } = "{ }";

        public Task<IConnector> BuildConnectorAsync(string apiKey, CancellationToken cancellationToken)
        {
            var connector = new DirectChronoTriggerConnector(_TriggerSchedulerFactory.GetTriggerScheduler(apiKey), _StringSerializer);

            return Task.FromResult<IConnector>(connector);
        }
    }
}
