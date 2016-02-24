using System;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Services
{
    public interface IScriptHost
    {
        Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code);

        Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string payload, ICodeWithDatabaseConfiguration code);

        Func<CancellationToken, Task<bool>> BuildAsyncPredicateScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code);
    }
}