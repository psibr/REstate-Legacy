using REstate.Configuration;
using REstate.Engine.Services;
using REstateClient;
using System;
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

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, Code code)
        {
            return async (cancellationToken) =>
            {
                await _chronoSession.AddChronoTrigger(machineInstance.MachineInstanceId, code.Body);
            };
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, string payload, Code code)
        {
            return async (cancellationToken) =>
            {
                await _chronoSession.AddChronoTrigger(machineInstance.MachineInstanceId, code.Body, payload);
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
