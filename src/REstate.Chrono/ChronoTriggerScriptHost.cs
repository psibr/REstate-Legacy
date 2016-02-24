using System;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Services;

namespace REstate.Chrono
{
    public class ChronoTriggerScriptHost
        : IScriptHost
    {
        private readonly string _apiKey;

        public ChronoTriggerScriptHost(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void Dispose()
        {
        }

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            throw new NotImplementedException();
        }

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string payload, ICodeWithDatabaseConfiguration code)
        {
            throw new NotImplementedException();
        }

        public Func<CancellationToken, Task<bool>> BuildAsyncPredicateScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            throw new NotImplementedException();
        }
    }
}