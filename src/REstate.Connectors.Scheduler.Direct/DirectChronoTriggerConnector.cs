using REstate.Engine.Services;
using REstate.Scheduler;
using REstate.Scheduling;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Connectors.Scheduler
{
    public class DirectChronoTriggerConnector
        : IConnector
    {
        private readonly TriggerScheduler _TriggerScheduler;

        private readonly StringSerializer _StringSerializer;

        public DirectChronoTriggerConnector(TriggerScheduler triggerScheduler, StringSerializer stringSerializer)
        {
            _TriggerScheduler = triggerScheduler;

            _StringSerializer = stringSerializer;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, State state, IDictionary<string, string> configuration)
        {
            return async (cancellationToken) =>
            {
                if (configuration == null)
                    throw new ArgumentNullException(nameof(configuration));

                var trigger = new ChronoTrigger(configuration);

                if (trigger.MachineInstanceId == null)
                    trigger.MachineInstanceId = machineInstance.MachineInstanceId;

                trigger.StateName = state.StateName;

                trigger.LastCommitTag = state.CommitTag;

                await _TriggerScheduler.ScheduleTrigger(trigger, cancellationToken);
            };
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, State state, string payload, IDictionary<string, string> configuration)
        {
            return async (cancellationToken) =>
            {
                var trigger = new ChronoTrigger(configuration);

                if (trigger.MachineInstanceId == null)
                    trigger.MachineInstanceId = machineInstance.MachineInstanceId;

                trigger.StateName = state.StateName;
                trigger.Payload = payload;

                trigger.LastCommitTag = state.CommitTag;

                await _TriggerScheduler.ScheduleTrigger(trigger, cancellationToken);
            };
        }

        public Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, IDictionary<string, string> configuration)
        {
            throw new NotSupportedException();
        }

        public static string ConnectorKey { get; } = "Delay";

        string IConnector.ConnectorKey => ConnectorKey;
    }
}
