using System;
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

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string payload, ICodeWithDatabaseConfiguration code) =>
            async ct => await CSharpScript.Create(code.CodeBody,
                ScriptOptions.Default
                    .WithReferences(typeof(Console).Assembly)
                    .WithReferences(typeof(HttpClient).Assembly)
                    .WithReferences(typeof(RoslynScriptHost).Assembly)
                    .WithImports("System", "System.Net.Http", "System.Text", "System.Net", "REstate.RoslynScripting"), typeof(RoslynScriptGlobals))
                .CreateDelegate()
                .Invoke(new RoslynScriptGlobals { Machine = machineInstance, Payload = payload }, ct);

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code) =>
            BuildAsyncActionScript(machineInstance, null, code);

        public Func<CancellationToken, Task<bool>> BuildAsyncPredicateScript(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code) =>
            async ct => await CSharpScript.Create<bool>(code.CodeBody,
                ScriptOptions.Default
                    .WithReferences(typeof(Console).Assembly)
                    .WithReferences(typeof(HttpClient).Assembly)
                    .WithReferences(typeof(RoslynScriptHost).Assembly)
                    .WithImports("System", "System.Net.Http","System.Text", "System.Net", "REstate.RoslynScripting"), typeof(RoslynScriptGlobals))
                .CreateDelegate()
                .Invoke(new RoslynScriptGlobals { Machine = machineInstance }, ct);

        public void Dispose()
        {
            //Nothing to do.
        }
    }
}