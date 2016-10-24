using REstate.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Services
{
    public interface IConnector
    {
        string ConnectorKey { get; }

        Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, Code code);

        Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, string payload, Code code);

        Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, Code code);
    }
}
