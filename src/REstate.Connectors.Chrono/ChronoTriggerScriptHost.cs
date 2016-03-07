using System;
using System.Threading;
using System.Threading.Tasks;
using REstate.Client.Chrono;
using REstate.Configuration;
using REstate.Services;

namespace REstate.Connectors.Chrono
{
    public class ChronoTriggerScriptHost
        : IScriptHost
    {
        private readonly IChronoSession _chronoSession;

        public ChronoTriggerScriptHost(IChronoSession chronoSession)
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

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            return async (cancellationToken) =>
            {
                await _chronoSession.AddChronoTrigger(machineInstance.MachineInstanceId, code.CodeBody);
            };
        }

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string payload, ICodeWithDatabaseConfiguration code)
        {
            return async (cancellationToken) =>
            {
                await _chronoSession.AddChronoTrigger(machineInstance.MachineInstanceId, code.CodeBody, payload);
            };
        }

        public Func<CancellationToken, Task<bool>> BuildAsyncPredicateScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            throw new NotSupportedException();
        }
    }
}