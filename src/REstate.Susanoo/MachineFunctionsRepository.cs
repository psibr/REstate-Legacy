using System.Data;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Repositories;
using Susanoo;

namespace REstate.Susanoo
{
    public class MachineFunctionsRepository
        : REstateContextualRepository, IMachineFunctionsRepository
    {
        public MachineFunctionsRepository(Repository root)
            : base(root)
        {
        }

        public async Task<bool> ExecuteGuardClause(ICodeWithDatabaseConfiguration code, CancellationToken cancellationToken)
        {
            using (var db = CommandManager.ResolveDatabaseManagerFactory()
                .CreateFromConnectionString(code.ConnectionString, code.ProviderValue))
                return await CommandManager.Instance
                    .DefineCommand(code.CodeBody, CommandType.Text)
                    .Realize()
                    .ExecuteScalarAsync<bool>(db, cancellationToken);
        }

        public async Task ExecuteActionClause(ICodeWithDatabaseConfiguration code, CancellationToken cancellationToken)
        {
            using (var db = CommandManager.ResolveDatabaseManagerFactory()
                .CreateFromConnectionString(code.ConnectionString, code.ProviderValue))
                await CommandManager.Instance
                    .DefineCommand(code.CodeBody, CommandType.Text)
                    .Realize()
                    .ExecuteNonQueryAsync(db, null, null, cancellationToken);
        }

        public async Task ExecuteActionClause(ICodeWithDatabaseConfiguration code, string payload, CancellationToken cancellationToken)
        {
            using (var db = CommandManager.ResolveDatabaseManagerFactory()
                .CreateFromConnectionString(code.ConnectionString, code.ProviderValue))
                await CommandManager.Instance
                    .DefineCommand(code.CodeBody, CommandType.Text)
                    .Realize()
                    .ExecuteNonQueryAsync(db, new { payload }, null, cancellationToken);
        }
    }
}