using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Services
{
    public interface IScriptHost : IDisposable
    {
        Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string script);

        Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string payload, string script);

        Func<CancellationToken, Task<bool>> BuildAsyncPredicateScript(IStateMachine machineInstance, string script);
    }
}