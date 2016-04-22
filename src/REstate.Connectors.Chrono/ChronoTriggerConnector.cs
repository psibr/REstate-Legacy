using REstate.Client.Chrono;
using REstate.Configuration;
using REstate.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Connectors.Chrono
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

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            return async (cancellationToken) =>
            {
                await _chronoSession.AddChronoTrigger(machineInstance.MachineInstanceId, code.CodeBody);
            };
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, string payload, ICodeWithDatabaseConfiguration code)
        {
            return async (cancellationToken) =>
            {
                await _chronoSession.AddChronoTrigger(machineInstance.MachineInstanceId, code.CodeBody, payload);
            };
        }

        public Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            throw new NotSupportedException();
        }

        public string ConnectorKey { get; } = "REstate.Connectors.Chrono";
    }
}