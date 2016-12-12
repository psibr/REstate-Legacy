using REstate.Engine;
using REstate.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Scheduler.Consumer.Direct
{
    public class DirectChronoConsumer : ChronoConsumer
    {
        protected StateEngineFactory StateEngineFactory { get; }

        protected StateEngine StateEngine { get; set; }

        public DirectChronoConsumer(
            IChronoRepositoryFactory repositoryFactory,
            TriggerSchedulerFactory triggerSchedulerFactory,
            IPlatformLogger logger,
            StateEngineFactory stateEngineFactory)
            : base(repositoryFactory, triggerSchedulerFactory, logger)
        {
            StateEngineFactory = stateEngineFactory;
        }

        protected override Task Initialize(string apiKey)
        {
            StateEngine = StateEngineFactory.GetStateEngine(apiKey);

            return Task.CompletedTask;
        }

        protected async override Task<State> GetStateAsync(string machineInstanceId, CancellationToken cancellationToken)
        {
            var record = await StateEngine.GetInstanceInfoAsync(machineInstanceId, cancellationToken).ConfigureAwait(false);

            return new State(record.MachineName, record.StateName, record.CommitTag);
        }

        protected async override Task FireTriggerAsync(string machineInstanceId, string triggerName, string contentType, string payload, string lastCommitTag, CancellationToken cancellationToken)
        {
            var machine = await StateEngine.GetInstance(machineInstanceId, cancellationToken);

            await machine.FireAsync(new Trigger(machine.MachineDefinitionId, triggerName), contentType, payload, lastCommitTag, cancellationToken);
        }
    }
}
