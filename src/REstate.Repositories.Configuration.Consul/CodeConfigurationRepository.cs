using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Repositories.Configuration;

namespace REstate.Repositories.Core.Consul
{
    public class CodeConfigurationRepository
        : CoreContextualRepository, ICodeConfigurationRepository
    {

        public CodeConfigurationRepository(CoreRepository parent)
            : base(parent)
        {
        }

        public Task<CodeElement> DefineCodeElement(CodeElement codeElement, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CodeElement> UpdateCodeElement(CodeElement codeElement, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ISqlDatabaseDefinition> DefineDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ISqlDatabaseDefinition> UpdateDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ISqlDatabaseDefinition>> GetDatabaseDefinitions(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}