using REstate.Configuration;
using REstate.Engine.Services;
using REstate.Scheduler;
using REstate.Scheduling;
using System;
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

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, Code code)
        {
            return async (cancellationToken) =>
            {
                var trigger = _StringSerializer.Deserialize<ChronoTrigger>(code.Body);

                await _TriggerScheduler.ScheduleTrigger(trigger, cancellationToken);
            };
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, string payload, Code code)
        {
            return async (cancellationToken) =>
            {
                var trigger = _StringSerializer.Deserialize<ChronoTrigger>(code.Body);

                trigger.Payload = payload;

                await _TriggerScheduler.ScheduleTrigger(trigger, cancellationToken);
            };
        }

        public Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, Code code)
        {
            throw new NotSupportedException();
        }

        public static string ConnectorKey { get; } = "REstate.Engine.Connectors.Scheduler";

        string IConnector.ConnectorKey => ConnectorKey;
    }
}
