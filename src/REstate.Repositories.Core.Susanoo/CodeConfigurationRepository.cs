using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using Susanoo;

namespace REstate.Repositories.Core.Susanoo
{
    public class CodeConfigurationRepository
        : ConfigurationContextualRepository, ICodeConfigurationRepository
    {
        public CodeConfigurationRepository(ConfigurationRepository root)
            : base(root)
        {
        }

        public async Task<CodeElement> DefineCodeElement(CodeElement codeElement, CancellationToken cancellationToken)
        {
            if (codeElement == null) throw new ArgumentNullException(nameof(codeElement));
            if (string.IsNullOrWhiteSpace(codeElement.ConnectorKey)) throw new ArgumentException("ConnectorKey is a required property.", nameof(codeElement));
            if (string.IsNullOrWhiteSpace(codeElement.CodeElementName)) throw new ArgumentException("CodeElementName is a required property.", nameof(codeElement));
            if (string.IsNullOrWhiteSpace(codeElement.SemanticVersion)) throw new ArgumentException("SemanticVersion is a required property.", nameof(codeElement));

            return (await CommandManager.Instance
                .DefineCommand<CodeElement>("INSERT INTO CodeElements VALUES (@ConnectorKey, @CodeElementName, @SemanticVersion,\n" +
                                            "    @CodeElementDescription, @CodeBody, @SqlDatabaseDefinitionId); \n" +
                                            "SELECT * FROM CodeElements WHERE CodeElementId = @@IDENTITY;",
                    CommandType.Text)
                .UseExplicitPropertyInclusionMode()
                .IncludeProperty(o => o.ConnectorKey)
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
            if (string.IsNullOrWhiteSpace(codeElement.ConnectorKey)) throw new ArgumentException("ConnectorKey is a required property.", nameof(codeElement));
            if (string.IsNullOrWhiteSpace(codeElement.CodeElementName)) throw new ArgumentException("CodeElementName is a required property.", nameof(codeElement));
            if (string.IsNullOrWhiteSpace(codeElement.SemanticVersion)) throw new ArgumentException("SemanticVersion is a required property.", nameof(codeElement));

            return (await CommandManager.Instance
                .DefineCommand<CodeElement>("UPDATE CodeElements SET ConnectorKey = @ConnectorKey, CodeElementName = @CodeElementName,\n" +
                                            "   SemanticVersion = @SemanticVersion, CodeElementDescription = @CodeElementDescription,\n" +
                                            "   CodeBody = @CodeBody, SqlDatabaseDefinitionId = @SqlDatabaseDefinitionId\n" +
                                            "WHERE CodeElementId = @CodeElementId; \n" +
                                            "SELECT * FROM CodeElements WHERE CodeElementId = @CodeElementId;",
                    CommandType.Text)
                .UseExplicitPropertyInclusionMode()
                .IncludeProperty(o => o.CodeElementId)
                .IncludeProperty(o => o.ConnectorKey)
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

        public async Task<ICollection<ISqlDatabaseDefinition>> GetDatabaseDefinitions(CancellationToken cancellationToken)
        {
            return (await CommandManager.Instance
                .DefineCommand("SELECT * FROM SqlDatabaseDefinitions", CommandType.Text)
                .DefineResults<SqlDatabaseDefinition>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, cancellationToken))
                .Cast<ISqlDatabaseDefinition>()
                .ToList();
        }

        public async Task<ISqlDatabaseDefinition> DefineDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition, CancellationToken cancellationToken)
        {
            if (databaseDefinition == null) throw new ArgumentNullException(nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.SqlDatabaseName)) throw new ArgumentException("SqlDatabaseName is a required property.", nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.ConnectionString)) throw new ArgumentException("ConnectionString is a required property.", nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.ProviderName)) throw new ArgumentException("ProviderName is a required property.", nameof(databaseDefinition));

            return (await CommandManager.Instance
                .DefineCommand<ISqlDatabaseDefinition>(
                    "INSERT INTO SqlDatabaseDefinitions \n" +
                    "VALUES(@SqlDatabaseName, @SqlDatabaseDescription, @ConnectionString, @ProviderName);" +
                    "\n\nSELECT * FROM SqlDatabaseDefinitions WHERE SqlDatabaseDefinitionId = @@IDENTITY", CommandType.Text)
                .DefineResults<SqlDatabaseDefinition>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, databaseDefinition, cancellationToken))
                .Single();
        }

        public async Task<ISqlDatabaseDefinition> UpdateDatabaseDefinition(ISqlDatabaseDefinition databaseDefinition, CancellationToken cancellationToken)
        {
            if (databaseDefinition == null) throw new ArgumentNullException(nameof(databaseDefinition));
            if (databaseDefinition.SqlDatabaseDefinitionId <= 0) throw new ArgumentException("SqlDatabaseDefinitionId is a required property.", nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.SqlDatabaseName)) throw new ArgumentException("SqlDatabaseName is a required property.", nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.ConnectionString)) throw new ArgumentException("ConnectionString is a required property.", nameof(databaseDefinition));
            if (string.IsNullOrWhiteSpace(databaseDefinition.ProviderName)) throw new ArgumentException("ProviderName is a required property.", nameof(databaseDefinition));

            return (await CommandManager.Instance
                .DefineCommand<ISqlDatabaseDefinition>(
                    "UPDATE SqlDatabaseDefinitions SET " +
                    "\nSqlDatabaseName = @SqlDatabaseName," +
                    "\nSqlDatabaseDescription = @SqlDatabaseDescription," +
                    "\nConnectionString = @ConnectionString," +
                    "\nProviderName = @ProviderName" +
                    "\nWHERE SqlDatabaseDefinitionId = @SqlDatabaseDefinitionId" +
                    "\n\nSELECT * FROM SqlDatabaseDefinitions WHERE SqlDatabaseDefinitionId = @SqlDatabaseDefinitionId",
                    CommandType.Text)
                .DefineResults<SqlDatabaseDefinition>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, databaseDefinition, cancellationToken))
                .Single();
        }
    }
}