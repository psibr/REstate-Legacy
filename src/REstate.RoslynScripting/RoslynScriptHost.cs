using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using REstate.RoslynScripting.Globals;
using REstate.Services;

namespace REstate.RoslynScripting
{
    public class RoslynScriptHost : IScriptHost
    {
        public RoslynScriptHost()
        {

        }

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string payload, string script) =>
            async ct => await CSharpScript.Create(script,
                ScriptOptions.Default
                    .WithReferences(typeof(Console).Assembly)
                    .WithReferences(typeof(HttpClient).Assembly)
                    .WithReferences(typeof(RoslynScriptHost).Assembly)
                    .WithImports("System", "System.Net.Http", "System.Text", "System.Net", "REstate.RoslynScripting"), typeof(RoslynScriptGlobals))
                .CreateDelegate()
                .Invoke(new RoslynScriptGlobals { Machine = machineInstance, Payload = payload }, ct);

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance, string script) =>
            BuildAsyncActionScript(machineInstance, null, script);

        public Func<CancellationToken, Task<bool>> BuildAsyncPredicateScript(IStateMachine machineInstance, string script) =>
            async ct => await CSharpScript.Create<bool>(script,
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