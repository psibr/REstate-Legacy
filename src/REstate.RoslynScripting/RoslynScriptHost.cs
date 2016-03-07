using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using REstate.Configuration;
using REstate.RoslynScripting.Globals;
using REstate.Services;

namespace REstate.RoslynScripting
{
    public class RoslynScriptHost : IScriptHost
    {
        private readonly string _apiKey;

        public RoslynScriptHost(string apiKey)
        {
            _apiKey = apiKey;
        }

        private static readonly IDictionary<string, ScriptRunner<object>> ActionDelegateCache =
            new Dictionary<string, ScriptRunner<object>>();

        private static readonly IDictionary<string, ScriptRunner<bool>> PredicateDelegateCache =
            new Dictionary<string, ScriptRunner<bool>>();

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string payload,
            ICodeWithDatabaseConfiguration code) =>
                async ct =>
                {
                    ScriptRunner<object> runner;
                    if (!ActionDelegateCache.TryGetValue(code.CodeBody, out runner))
                    {
                        runner = CSharpScript.Create(code.CodeBody,
                            ScriptOptions.Default
                                .WithReferences(typeof (Console).Assembly)
                                .WithReferences(typeof (HttpClient).Assembly)
                                .WithReferences(typeof (RoslynScriptHost).Assembly)
                                .WithImports("System", "System.Net.Http", "System.Text", "System.Net",
                                    "REstate.RoslynScripting"),
                            typeof (RoslynScriptGlobals))
                            .CreateDelegate();

                        ActionDelegateCache.Add(code.CodeBody, runner);
                    }

                    await runner
                        .Invoke(new RoslynScriptGlobals { Machine = machineInstance, Payload = payload }, ct);
                };

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code) =>
            BuildAsyncActionScript(machineInstance, null, code);

        public Func<CancellationToken, Task<bool>> BuildAsyncPredicateScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code) =>
                async ct =>
                {
                    ScriptRunner<bool> runner;
                    if (!PredicateDelegateCache.TryGetValue(code.CodeBody, out runner))
                    {
                        runner = CSharpScript.Create<bool>(code.CodeBody,
                            ScriptOptions.Default
                                .WithReferences(typeof(Console).Assembly)
                                .WithReferences(typeof(HttpClient).Assembly)
                                .WithReferences(typeof(RoslynScriptHost).Assembly)
                                .WithImports("System", "System.Net.Http", "System.Text", "System.Net",
                                    "REstate.RoslynScripting"),
                            typeof(RoslynScriptGlobals))
                            .CreateDelegate();

                        PredicateDelegateCache.Add(code.CodeBody, runner);
                    }

                    return await runner
                        .Invoke(new RoslynScriptGlobals { Machine = machineInstance }, ct);
                };

        public void Dispose()
        {
            //Nothing to do.
        }
    }
}