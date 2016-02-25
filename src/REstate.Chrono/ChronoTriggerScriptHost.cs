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
        private readonly IChronoEngine _engine;
        private readonly IJsonSerializer _json;
        private readonly string _apiKey;

        public ChronoTriggerScriptHost(IChronoEngine engine, IJsonSerializer json, string apiKey)
        {
            _engine = engine;
            _json = json;
            _apiKey = apiKey;
        }

        public void Dispose()
        {
        }

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            return async (cancellationToken) =>
            {
                IChronoTrigger trigger = _json.Deserialize<ChronoTrigger>(code.CodeBody);
                trigger.MachineDefinitionId = machineInstance.MachineDefinitionId;
                trigger.MachineInstanceId = machineInstance.MachineInstanceId;
                await _engine.AddChronoTrigger(trigger, cancellationToken);
            };
        }

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string payload, ICodeWithDatabaseConfiguration code)
        {
            throw new NotSupportedException();
        }

        public Func<CancellationToken, Task<bool>> BuildAsyncPredicateScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            throw new NotSupportedException();
        }
    }
}