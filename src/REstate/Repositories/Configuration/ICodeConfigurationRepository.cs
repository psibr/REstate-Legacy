using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Repositories.Configuration
{
    public interface ICodeConfigurationRepository
    {
        Task<ICollection<CodeType>> GetCodeTypes(CancellationToken cancellationToken);

        Task<ICollection<CodeType>> GetCodeTypes(int codeUsageId, CancellationToken cancellationToken);

        Task<ICollection<CodeType>> GetCodeTypes(string codeUsageName, CancellationToken cancellationToken);

        Task<ICollection<CodeUsage>> GetCodeUsages(CancellationToken cancellationToken);

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