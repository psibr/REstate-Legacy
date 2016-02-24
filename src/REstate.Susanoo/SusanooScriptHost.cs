using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Services;
using Susanoo;

namespace REstate.Susanoo
{
    public class SusanooScriptHost
        : IScriptHost
        
    {
        private readonly string _apiKey;

        public SusanooScriptHost(string apiKey)
        {
            _apiKey = apiKey;
        }

        protected async Task<bool> ExecuteGuardClause(ICodeWithDatabaseConfiguration code, CancellationToken cancellationToken)
        {
            using (var db = CommandManager.ResolveDatabaseManagerFactory()
                .CreateFromConnectionString(code.ConnectionString, code.ProviderValue))
                return await CommandManager.Instance
                    .DefineCommand(code.CodeBody, CommandType.Text)
                    .Realize()
                    .ExecuteScalarAsync<bool>(db, cancellationToken);
        }

        protected async Task ExecuteActionClause(ICodeWithDatabaseConfiguration code, CancellationToken cancellationToken)
        {
            using (var db = CommandManager.ResolveDatabaseManagerFactory()
                .CreateFromConnectionString(code.ConnectionString, code.ProviderValue))
                await CommandManager.Instance
                    .DefineCommand(code.CodeBody, CommandType.Text)
                    .Realize()
                    .ExecuteNonQueryAsync(db, null, null, cancellationToken);
        }

        protected async Task ExecuteActionClause(ICodeWithDatabaseConfiguration code, string payload, CancellationToken cancellationToken)
        {
            using (var db = CommandManager.ResolveDatabaseManagerFactory()
                .CreateFromConnectionString(code.ConnectionString, code.ProviderValue))
                await CommandManager.Instance
                    .DefineCommand(code.CodeBody, CommandType.Text)
                    .Realize()
                    .ExecuteNonQueryAsync(db, new { payload }, null, cancellationToken);
        }

        public void Dispose()
        {
        }

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance,
            ICodeWithDatabaseConfiguration code) => async (cancellationToken) =>
                await ExecuteActionClause(code, cancellationToken);

        public Func<CancellationToken, Task> BuildAsyncActionScript(IStateMachine machineInstance,
            string payload,
            ICodeWithDatabaseConfiguration code) => async (cancellationToken) => 
                await ExecuteActionClause(code, payload, cancellationToken);

        public Func<CancellationToken, Task<bool>> BuildAsyncPredicateScript(IStateMachine machineInstance,
            ICodeWithDatabaseConfiguration code) => async (cancellationToken) =>
                await ExecuteGuardClause(code, cancellationToken);
    }
}