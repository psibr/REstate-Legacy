using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using REstate.Configuration;
using REstate.Repositories;

namespace REstate.Web.Modules
{
    /// <summary>
    /// Executable code elements configuration module.
    /// </summary>
    public class CodeElementsModule
        : ConfigurationModule
    {
        /// <summary>
        /// Registers routes for modifying executable code.
        /// </summary>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        public CodeElementsModule(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
            : base("/code", "developer")
        {
            GetCodeUsages(configurationRepositoryContextFactory);

            GetCodeTypes(configurationRepositoryContextFactory);

            GetCodeTypesByUsageId(configurationRepositoryContextFactory);

            GetCodeTypesByUsageName(configurationRepositoryContextFactory);

            CreateCodeElement(configurationRepositoryContextFactory);

            UpdateCodeElement(configurationRepositoryContextFactory);

            GetDatabaseDefinitions(configurationRepositoryContextFactory);

            GetDatabaseProviders(configurationRepositoryContextFactory);

            DefineDatabaseDefinition(configurationRepositoryContextFactory);

            UpdateDatabaseDefinition(configurationRepositoryContextFactory);
        }

        private void UpdateDatabaseDefinition(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Put["UpdateDatabaseDefinition", "/databasedefinitions/", true] = async (parameters, ct) =>
            {
                ISqlDatabaseDefinition defintion = this.Bind<SqlDatabaseDefinition>();

                ISqlDatabaseDefinitionAndProvider databaseDefinitionAndProvider;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    databaseDefinitionAndProvider = await repository.Code.UpdateDatabaseDefinition(defintion, ct);
                }

                return Negotiate
                    .WithModel(databaseDefinitionAndProvider)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineDatabaseDefinition(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineDatabaseDefinition", "/databasedefinitions/", true] = async (parameters, ct) =>
            {
                ISqlDatabaseDefinition defintion = this.Bind<SqlDatabaseDefinition>(/*ignore: */ (o) => o.SqlDatabaseDefinitionId);

                ISqlDatabaseDefinitionAndProvider databaseDefinitionAndProvider;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    databaseDefinitionAndProvider = await repository.Code.DefineDatabaseDefinition(defintion, ct);
                }

                return Negotiate
                    .WithModel(databaseDefinitionAndProvider)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetDatabaseProviders(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Get["GetDatabaseProviders", "/databaseproviders/", true] = async (parameters, ct) =>
            {
                ICollection<ISqlDatabaseProvider> sqlDatabaseProviders;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    sqlDatabaseProviders = await repository.Code.GetDatabaseProviders(ct);
                }

                return Negotiate
                    .WithModel(sqlDatabaseProviders)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetDatabaseDefinitions(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Get["GetDatabaseDefinitions", "/databasedefinitions/", true] = async (parameters, ct) =>
            {
                ICollection<ISqlDatabaseDefinitionAndProvider> databaseDefinition;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    databaseDefinition = await repository.Code.GetDatabaseDefinitions(ct);
                }

                return Negotiate
                    .WithModel(databaseDefinition)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void UpdateCodeElement(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Put["UpdateCodeElement", "/elements/", true] = async (parameters, ct) =>
            {
                var codeElement = this.Bind<CodeElement>();

                CodeElement newCodeElement;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newCodeElement = await repository.Code.UpdateCodeElement(codeElement, ct);
                }

                return Negotiate
                    .WithModel(newCodeElement)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void CreateCodeElement(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineCodeElement", "/elements/", true] = async (parameters, ct) =>
            {
                var codeElement = this.Bind<CodeElement>(/*ignore: */ e => e.CodeElementId);

                CodeElement newCodeElement;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newCodeElement = await repository.Code.DefineCodeElement(codeElement, ct);
                }

                return Negotiate
                    .WithModel(newCodeElement)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetCodeTypesByUsageName(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Get["GetCodeTypesByUsageName", "/types/{CodeUsageName}", true] = async (parameters, ct) =>
            {
                string codeUsageName = parameters.CodeUsageName;

                ICollection<CodeType> codeTypes;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    codeTypes = await repository.Code.GetCodeTypes(codeUsageName, ct);
                }

                return Negotiate
                    .WithModel(codeTypes)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetCodeTypesByUsageId(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Get["GetCodeTypesByUsageId", "/types/{CodeUsageId:int}", true] = async (parameters, ct) =>
            {
                int codeUsageId = parameters.CodeUsageId;

                ICollection<CodeType> codeTypes;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    codeTypes = await repository.Code.GetCodeTypes(codeUsageId, ct);
                }

                return Negotiate
                    .WithModel(codeTypes)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetCodeTypes(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Get["GetCodeTypes", "/types", true] = async (parameters, ct) =>
            {
                ICollection<CodeType> codeTypes;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    codeTypes = await repository.Code.GetCodeTypes(ct);
                }

                return Negotiate
                    .WithModel(codeTypes)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetCodeUsages(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Get["GetCodeUsages", "/usages", true] = async (parameters, ct) =>
            {
                ICollection<CodeUsage> codeUsages;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    codeUsages = await repository.Code.GetCodeUsages(ct);
                }

                return Negotiate
                    .WithModel(codeUsages)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }
    }
}
