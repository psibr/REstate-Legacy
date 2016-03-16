using REstate.Configuration;
using REstate.Services;
using Susanoo;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Connectors.Susanoo
{
    public class SusanooConnector
        : IConnector
        
    {
        private readonly string _apiKey;

        public SusanooConnector(string apiKey)
        {
            _apiKey = apiKey;
        }

        protected async Task<bool> ExecuteGuardClause(ICodeWithDatabaseConfiguration code, CancellationToken cancellationToken)
        {
            using (var db = CommandManager.ResolveDatabaseManagerFactory()
                .CreateFromConnectionString(code.ConnectionString, code.ProviderName))
                return await CommandManager.Instance
                    .DefineCommand(code.CodeBody, CommandType.Text)
                    .Realize()
                    .ExecuteScalarAsync<bool>(db, cancellationToken);
        }

        protected async Task ExecuteActionClause(ICodeWithDatabaseConfiguration code, CancellationToken cancellationToken)
        {
            using (var db = CommandManager.ResolveDatabaseManagerFactory()
                .CreateFromConnectionString(code.ConnectionString, code.ProviderName))
                await CommandManager.Instance
                    .DefineCommand(code.CodeBody, CommandType.Text)
                    .Realize()
                    .ExecuteNonQueryAsync(db, null, null, cancellationToken);
        }

        protected async Task ExecuteActionClause(ICodeWithDatabaseConfiguration code, string payload, CancellationToken cancellationToken)
        {
            using (var db = CommandManager.ResolveDatabaseManagerFactory()
                .CreateFromConnectionString(code.ConnectionString, code.ProviderName))
                await CommandManager.Instance
                    .DefineCommand(code.CodeBody, CommandType.Text)
                    .Realize()
                    .ExecuteNonQueryAsync(db, new { payload }, null, cancellationToken);
        }

        public void Dispose()
        {
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance,
            ICodeWithDatabaseConfiguration code) => async (cancellationToken) =>
                await ExecuteActionClause(code, cancellationToken);

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance,
            string payload,
            ICodeWithDatabaseConfiguration code) => async (cancellationToken) => 
                await ExecuteActionClause(code, payload, cancellationToken);

        public Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance,
            ICodeWithDatabaseConfiguration code) => async (cancellationToken) =>
                await ExecuteGuardClause(code, cancellationToken);
    }
}