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

        Task<ICollection<ISqlDatabaseDefinitionAndProvider>> GetDatabaseDefinitions(CancellationToken cancellationToken);

        Task<ICollection<ISqlDatabaseProvider>> GetDatabaseProviders(CancellationToken cancellationToken);

        Task<ISqlDatabaseDefinitionAndProvider> DefineDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition,
            CancellationToken cancellationToken);

        Task<ISqlDatabaseDefinitionAndProvider> UpdateDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition,
            CancellationToken cancellationToken);
    }
}