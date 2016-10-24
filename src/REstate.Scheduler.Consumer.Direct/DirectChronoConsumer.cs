using REstate.Configuration;
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

        protected async override Task<InstanceRecord> GetState(string machineInstanceId)
        {
            return await StateEngine.GetInstanceInfo(machineInstanceId, CancellationToken.None);
        }

        protected async override Task FireTrigger(string machineInstanceId, string triggerName, string payload)
        {
            var machine = await StateEngine.GetInstance(machineInstanceId, CancellationToken.None);

            machine.Fire(new Trigger(machine.MachineDefinitionId, triggerName), payload);
        }
    }
}
