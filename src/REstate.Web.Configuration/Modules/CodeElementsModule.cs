using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using System.Collections.Generic;

namespace REstate.Web.Configuration.Modules
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
        /// <param name="prefix">The route prefix.</param>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        public CodeElementsModule(ConfigurationRoutePrefix prefix,
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
            : base(prefix + "/code", "developer")
        {
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
    }
}
