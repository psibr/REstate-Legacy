using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using REstate.Configuration;
using REstate.Connectors.RoslynScripting.Globals;
using REstate.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Psibr.Platform.Logging;

namespace REstate.Connectors.RoslynScripting
{
    public class RoslynConnector : IConnector
    {
        private readonly IPlatformLogger _logger;
        private readonly string _apiKey;

        public RoslynConnector(IPlatformLogger logger, string apiKey)
        {
            _logger = logger;
            _apiKey = apiKey;
        }

        private static readonly IDictionary<string, ScriptRunner<object>> ActionDelegateCache =
            new Dictionary<string, ScriptRunner<object>>();

        private static readonly IDictionary<string, ScriptRunner<bool>> PredicateDelegateCache =
            new Dictionary<string, ScriptRunner<bool>>();

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, string payload,
            Code code) =>
                async ct =>
                {
                    ScriptRunner<object> runner;
                    if (!ActionDelegateCache.TryGetValue(code.Body, out runner))
                    {
                        runner = CSharpScript.Create(code.Body,
                            ScriptOptions.Default
                                .WithReferences(typeof (Console).Assembly)
                                .WithReferences(typeof (HttpClient).Assembly)
                                .WithImports("System", "System.Net.Http", "System.Text", "System.Net"),
                            typeof (RoslynScriptGlobals))
                            .CreateDelegate();

                        ActionDelegateCache.Add(code.Body, runner);
                    }

                    await runner
                        .Invoke(new RoslynScriptGlobals
                        {
                            Machine = machineInstance,
                            Payload = payload,
                            Logger = _logger
                                .ForContext("MachineDefinitionId", machineInstance.MachineDefinitionId)
                                .ForContext("MachineInstanceId", machineInstance.MachineInstanceId)
                        }, ct);
                };

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, Code code) =>
            ConstructAction(machineInstance, null, code);

        public Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, Code code) =>
                async ct =>
                {
                    ScriptRunner<bool> runner;
                    if (!PredicateDelegateCache.TryGetValue(code.Body, out runner))
                    {
                        runner = CSharpScript.Create<bool>(code.Body,
                            ScriptOptions.Default
                                .WithReferences(typeof(Console).Assembly)
                                .WithReferences(typeof(HttpClient).Assembly)
                                .WithImports("System", "System.Net.Http", "System.Text", "System.Net"),
                            typeof(RoslynScriptGlobals))
                            .CreateDelegate();

                        PredicateDelegateCache.Add(code.Body, runner);
                    }

                    return await runner
                        .Invoke(new RoslynScriptGlobals
                        {
                            Machine = machineInstance,
                            Logger = _logger
                                .ForContext("MachineDefinitionId", machineInstance.MachineDefinitionId)
                                .ForContext("MachineInstanceId", machineInstance.MachineInstanceId)
                        }, ct);
                };

        public string ConnectorKey { get; } = "REstate.Connectors.RoslynScripting";

        public void Dispose()
        {
            //Nothing to do.
        }
    }
}