using REstate.Engine;
using REstate.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;

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
            var record = await StateEngine.GetMachineInfoAsync(machineInstanceId, cancellationToken).ConfigureAwait(false);

            return new State(record.StateName, Guid.Parse(record.CommitTag));
        }

        protected async override Task FireTriggerAsync(string machineInstanceId, string triggerName, string contentType, string payload, Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            var machine = await StateEngine.GetMachine(machineInstanceId, cancellationToken);

            await machine.FireAsync(new Trigger(triggerName), contentType, payload, lastCommitTag, cancellationToken);
        }
    }
}
