using REstate.Configuration;
using REstate.Engine.Services;
using REstate.Scheduling;
using REstateClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Connectors.Scheduler
{
    public class ChronoTriggerConnector
        : IConnector
    {
        private readonly IChronoSession _chronoSession;

        public ChronoTriggerConnector(IChronoSession chronoSession)
        {
            _chronoSession = chronoSession;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _chronoSession.Dispose();
            }
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, State state, IDictionary<string, string> configuration)
        {
            return async (cancellationToken) =>
            {
                var trigger = new ChronoTrigger(configuration);

                if (trigger.MachineInstanceId == null)
                    trigger.MachineInstanceId = machineInstance.MachineInstanceId;

                trigger.StateName = state.StateName;

                trigger.LastCommitTag = state.CommitTag;

                await _chronoSession.AddChronoTrigger(trigger);
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

                await _chronoSession.AddChronoTrigger(trigger);
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
