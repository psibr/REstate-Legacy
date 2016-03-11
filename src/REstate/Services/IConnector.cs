using REstate.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Services
{
    public interface IConnector
    {
        Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code);

        Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, string payload, ICodeWithDatabaseConfiguration code);

        Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code);
    }
}