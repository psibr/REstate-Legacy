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

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, State state, string contentType, string payload, IDictionary<string, string> configuration)
        {
            return async (cancellationToken) =>
            {
                var trigger = ChronoTrigger.FromConfiguration(configuration);

                if (trigger.MachineInstanceId == null)
                    trigger.MachineInstanceId = machineInstance.MachineId;

                trigger.StateName = state.StateName;
                trigger.ContentType = contentType;
                trigger.Payload = payload;

                trigger.LastCommitTag = state.CommitTag;

                await _chronoSession.AddChronoTriggerAsync(trigger, cancellationToken);
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
