using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Repositories.Configuration
{
    public interface ICodeConfigurationRepository
    {
        Task<CodeElement> DefineCodeElement(CodeElement codeElement, CancellationToken cancellationToken);

        Task<CodeElement> UpdateCodeElement(CodeElement codeElement, CancellationToken cancellationToken);

        Task<ICollection<ISqlDatabaseDefinition>> GetDatabaseDefinitions(CancellationToken cancellationToken);

        Task<ISqlDatabaseDefinition> DefineDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition,
            CancellationToken cancellationToken);

        Task<ISqlDatabaseDefinition> UpdateDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition,
            CancellationToken cancellationToken);
    }
}