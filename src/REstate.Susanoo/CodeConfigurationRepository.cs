using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Repositories;
using Susanoo;

namespace REstate.Susanoo
{
    public class CodeConfigurationRepository
        : REstateContextualRepository, ICodeConfigurationRepository
    {
        public CodeConfigurationRepository(Repository root)
            : base(root)
        {
        }

        public async Task<ICollection<CodeType>> GetCodeTypes(CancellationToken cancellationToken)
        {
            return (await CommandManager.Instance
                .DefineCommand("SELECT * FROM CodeTypes", CommandType.Text)
                .DefineResults<CodeType>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, cancellationToken))
                .ToList();
        }

        public async Task<ICollection<CodeType>> GetCodeTypes(int codeUsageId, CancellationToken cancellationToken)
        {
            return (await CommandManager.Instance
                .DefineCommand("SELECT CodeTypes.* FROM CodeTypes " +
                               "INNER JOIN CodeTypeUsages ctu ON ctu.CodeTypeId = CodeTypes.CodeTypeId " +
                               "WHERE ctu.CodeUsageId = @codeUsageId", CommandType.Text)
                .DefineResults<CodeType>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { codeUsageId }, cancellationToken))
                .ToList();
        }

        public async Task<ICollection<CodeType>> GetCodeTypes(string codeUsageName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(codeUsageName))
                throw new ArgumentException("Argument is null or whitespace", nameof(codeUsageName));

            return (await CommandManager.Instance
                .DefineCommand("SELECT CodeTypes.* FROM CodeTypes " +
                               "INNER JOIN CodeTypeUsages ctu ON ctu.CodeTypeId = CodeTypes.CodeTypeId " +
                               "INNER JOIN CodeUsages u ON u.CodeUsageId = ctu.CodeUsageId " +
                               "WHERE AND u.CodeUsageName = @codeUsageName", CommandType.Text)
                .DefineResults<CodeType>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { codeUsageName }, cancellationToken))
                .ToList();
        }

        public async Task<ICollection<CodeUsage>> GetCodeUsages(CancellationToken cancellationToken)
        {

            return (await CommandManager.Instance
                .DefineCommand("SELECT CodeUsages.* FROM CodeUsages ", CommandType.Text)
                .DefineResults<CodeUsage>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, cancellationToken))
                .ToList();
        }

        public async Task<CodeElement> DefineCodeElement(CodeElement codeElement, CancellationToken cancellationToken)
        {
            if (codeElement == null) throw new ArgumentNullException(nameof(codeElement));
            if (codeElement.CodeTypeId <= 0) throw new ArgumentException("CodeTypeId is a required property.", nameof(codeElement));
            if (string.IsNullOrWhiteSpace(codeElement.CodeElementName)) throw new ArgumentException("CodeElementName is a required property.", nameof(codeElement));
            if (string.IsNullOrWhiteSpace(codeElement.SemanticVersion)) throw new ArgumentException("SemanticVersion is a required property.", nameof(codeElement));

            return (await CommandManager.Instance
                .DefineCommand<CodeElement>("INSERT INTO CodeElements VALUES (@CodeTypeId, @CodeElementName, @SemanticVersion,\n" +
                                            "    @CodeElementDescription, @CodeBody, @SqlDatabaseDefinitionId); \n" +
                                            "SELECT * FROM CodeElements WHERE CodeElementId = @@IDENTITY;",
                    CommandType.Text)
                .UseExplicitPropertyInclusionMode()
                .IncludeProperty(o => o.CodeTypeId)
                .IncludeProperty(o => o.CodeElementName)
                .IncludeProperty(o => o.SemanticVersion)
                .IncludeProperty(o => o.CodeElementDescription)
                .IncludeProperty(o => o.CodeBody)
                .IncludeProperty(o => o.SqlDatabaseDefinitionId)
                .SendNullValues()
                .DefineResults<CodeElement>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, codeElement, cancellationToken))
                .Single();
        }

        public async Task<CodeElement> UpdateCodeElement(CodeElement codeElement, CancellationToken cancellationToken)
        {
            if (codeElement == null) throw new ArgumentNullException(nameof(codeElement));
            if (codeElement.CodeElementId <= 0) throw new ArgumentException("CodeElementId is a required property.", nameof(codeElement));
            if (codeElement.CodeTypeId <= 0) throw new ArgumentException("CodeTypeId is a required property.", nameof(codeElement));
            if (string.IsNullOrWhiteSpace(codeElement.CodeElementName)) throw new ArgumentException("CodeElementName is a required property.", nameof(codeElement));
            if (string.IsNullOrWhiteSpace(codeElement.SemanticVersion)) throw new ArgumentException("SemanticVersion is a required property.", nameof(codeElement));

            return (await CommandManager.Instance
                .DefineCommand<CodeElement>("UPDATE CodeElements SET CodeTypeId = @CodeTypeId, CodeElementName = @CodeElementName,\n" +
                                            "   SemanticVersion = @SemanticVersion, CodeElementDescription = @CodeElementDescription,\n" +
                                            "   CodeBody = @CodeBody, SqlDatabaseDefinitionId = @SqlDatabaseDefinitionId\n" +
                                            "WHERE CodeElementId = @CodeElementId; \n" +
                                            "SELECT * FROM CodeElements WHERE CodeElementId = @CodeElementId;",
                    CommandType.Text)
                .UseExplicitPropertyInclusionMode()
                .IncludeProperty(o => o.CodeElementId)
                .IncludeProperty(o => o.CodeTypeId)
                .IncludeProperty(o => o.CodeElementName)
                .IncludeProperty(o => o.SemanticVersion)
                .IncludeProperty(o => o.CodeElementDescription)
                .IncludeProperty(o => o.CodeBody)
                .IncludeProperty(o => o.SqlDatabaseDefinitionId)
                .SendNullValues()
                .DefineResults<CodeElement>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, codeElement, cancellationToken))
                .Single();
        }

        public async Task<ICollection<ISqlDatabaseDefinitionAndProvider>> GetDatabaseDefinitions(CancellationToken cancellationToken)
        {
            return (await CommandManager.Instance
                .DefineCommand("SELECT * FROM SqlDatabaseDefinitionsAndProvider", CommandType.Text)
                .DefineResults<SqlDatabaseDefinitionAndProvider>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, cancellationToken))
                .Cast<ISqlDatabaseDefinitionAndProvider>()
                .ToList();
        }

        public async Task<ICollection<ISqlDatabaseProvider>> GetDatabaseProviders(CancellationToken cancellationToken)
        {
            return (await CommandManager.Instance
                .DefineCommand("SELECT * FROM SqlDatabaseProviders", CommandType.Text)
                .DefineResults<SqlDatabaseProvider>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, cancellationToken))
                .Cast<ISqlDatabaseProvider>()
                .ToList();
        }

        public async Task<ISqlDatabaseDefinitionAndProvider> DefineDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition, CancellationToken cancellationToken)
        {
            if (databaseDefinition == null) throw new ArgumentNullException(nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.SqlDatabaseName)) throw new ArgumentException("SqlDatabaseName is a required property.", nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.ConnectionString)) throw new ArgumentException("ConnectionString is a required property.", nameof(databaseDefinition));
            if (databaseDefinition.SqlDatabaseProviderId <= 0) throw new ArgumentException("SqlDatabaseProviderId is a required property.", nameof(databaseDefinition));

            return (await CommandManager.Instance
                .DefineCommand<ISqlDatabaseDefinition>(
                    "INSERT INTO SqlDatabaseDefinitions \n" +
                    "VALUES(@SqlDatabaseName, @SqlDatabaseDescription, @ConnectionString, @SqlDatabaseProviderId);" +
                    "\n\nSELECT * FROM SqlDatabaseDefinitionsAndProvider WHERE SqlDatabaseDefinitionId = @@IDENTITY", CommandType.Text)
                .DefineResults<SqlDatabaseDefinitionAndProvider>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, databaseDefinition, cancellationToken))
                .Single();
        }

        public async Task<ISqlDatabaseDefinitionAndProvider> UpdateDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition, CancellationToken cancellationToken)
        {
            if (databaseDefinition == null) throw new ArgumentNullException(nameof(databaseDefinition));
            if (databaseDefinition.SqlDatabaseDefinitionId <= 0) throw new ArgumentException("SqlDatabaseDefinitionId is a required property.", nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.SqlDatabaseName)) throw new ArgumentException("SqlDatabaseName is a required property.", nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.ConnectionString)) throw new ArgumentException("ConnectionString is a required property.", nameof(databaseDefinition));
            if (databaseDefinition.SqlDatabaseProviderId <= 0) throw new ArgumentException("SqlDatabaseProviderId is a required property.", nameof(databaseDefinition));

            return (await CommandManager.Instance
                .DefineCommand<ISqlDatabaseDefinition>(
                    "UPDATE SqlDatabaseDefinitions SET " +
                    "\nSqlDatabaseName = @SqlDatabaseName," +
                    "\nSqlDatabaseDescription = @SqlDatabaseDescription," +
                    "\nConnectionString = @ConnectionString," +
                    "\nSqlDatabaseProviderId = SqlDatabaseProviderId" +
                    "\nWHERE SqlDatabaseDefinitionId = @SqlDatabaseDefinitionId" +
                    "\n\nSELECT * FROM SqlDatabaseDefinitionsAndProvider WHERE SqlDatabaseDefinitionId = @SqlDatabaseDefinitionId",
                    CommandType.Text)
                .DefineResults<SqlDatabaseDefinitionAndProvider>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, databaseDefinition, cancellationToken))
                .Single();
        }
    }
}