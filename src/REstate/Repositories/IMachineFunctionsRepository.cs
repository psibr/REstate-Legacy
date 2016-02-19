using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Repositories
{
    public interface IMachineFunctionsRepository
    {
        Task<bool> ExecuteGuardClause(ICodeWithDatabaseConfiguration code, CancellationToken cancellationToken);

        Task ExecuteActionClause(ICodeWithDatabaseConfiguration code, CancellationToken cancellationToken);

        Task ExecuteActionClause(ICodeWithDatabaseConfiguration code, string payload, CancellationToken cancellationToken);
    }
}
