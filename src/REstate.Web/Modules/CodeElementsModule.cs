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
        /// <param name="repositoryContextFactory">The repository context factory.</param>
        public CodeElementsModule(IRepositoryContextFactory repositoryContextFactory)
            : base("/code", "developer")
        {
            GetCodeUsages(repositoryContextFactory);

            GetCodeTypes(repositoryContextFactory);

            GetCodeTypesByUsageId(repositoryContextFactory);

            GetCodeTypesByUsageName(repositoryContextFactory);

            CreateCodeElement(repositoryContextFactory);

            UpdateCodeElement(repositoryContextFactory);

            GetDatabaseDefinitions(repositoryContextFactory);

            GetDatabaseProviders(repositoryContextFactory);

            DefineDatabaseDefinition(repositoryContextFactory);

            UpdateDatabaseDefinition(repositoryContextFactory);
        }

        private void UpdateDatabaseDefinition(IRepositoryContextFactory repositoryContextFactory)
        {
            Put["UpdateDatabaseDefinition", "/databasedefinitions/", true] = async (parameters, ct) =>
            {
                ISqlDatabaseDefinition defintion = this.Bind<SqlDatabaseDefinition>();

                ISqlDatabaseDefinitionAndProvider databaseDefinitionAndProvider;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    databaseDefinitionAndProvider = await repository.Configuration.Code.UpdateDatabaseDefinition(defintion, ct);
                }

                return Negotiate
                    .WithModel(databaseDefinitionAndProvider)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineDatabaseDefinition(IRepositoryContextFactory repositoryContextFactory)
        {
            Post["DefineDatabaseDefinition", "/databasedefinitions/", true] = async (parameters, ct) =>
            {
                ISqlDatabaseDefinition defintion = this.Bind<SqlDatabaseDefinition>(/*ignore: */ (o) => o.SqlDatabaseDefinitionId);

                ISqlDatabaseDefinitionAndProvider databaseDefinitionAndProvider;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    databaseDefinitionAndProvider = await repository.Configuration.Code.DefineDatabaseDefinition(defintion, ct);
                }

                return Negotiate
                    .WithModel(databaseDefinitionAndProvider)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetDatabaseProviders(IRepositoryContextFactory repositoryContextFactory)
        {
            Get["GetDatabaseProviders", "/databaseproviders/", true] = async (parameters, ct) =>
            {
                ICollection<ISqlDatabaseProvider> sqlDatabaseProviders;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    sqlDatabaseProviders = await repository.Configuration.Code.GetDatabaseProviders(ct);
                }

                return Negotiate
                    .WithModel(sqlDatabaseProviders)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetDatabaseDefinitions(IRepositoryContextFactory repositoryContextFactory)
        {
            Get["GetDatabaseDefinitions", "/databasedefinitions/", true] = async (parameters, ct) =>
            {
                ICollection<ISqlDatabaseDefinitionAndProvider> databaseDefinition;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    databaseDefinition = await repository.Configuration.Code.GetDatabaseDefinitions(ct);
                }

                return Negotiate
                    .WithModel(databaseDefinition)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void UpdateCodeElement(IRepositoryContextFactory repositoryContextFactory)
        {
            Put["UpdateCodeElement", "/elements/", true] = async (parameters, ct) =>
            {
                var codeElement = this.Bind<CodeElement>();

                CodeElement newCodeElement;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newCodeElement = await repository.Configuration.Code.UpdateCodeElement(codeElement, ct);
                }

                return Negotiate
                    .WithModel(newCodeElement)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void CreateCodeElement(IRepositoryContextFactory repositoryContextFactory)
        {
            Post["DefineCodeElement", "/elements/", true] = async (parameters, ct) =>
            {
                var codeElement = this.Bind<CodeElement>(/*ignore: */ e => e.CodeElementId);

                CodeElement newCodeElement;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newCodeElement = await repository.Configuration.Code.DefineCodeElement(codeElement, ct);
                }

                return Negotiate
                    .WithModel(newCodeElement)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetCodeTypesByUsageName(IRepositoryContextFactory repositoryContextFactory)
        {
            Get["GetCodeTypesByUsageName", "/types/{CodeUsageName}", true] = async (parameters, ct) =>
            {
                string codeUsageName = parameters.CodeUsageName;

                ICollection<CodeType> codeTypes;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    codeTypes = await repository.Configuration.Code.GetCodeTypes(codeUsageName, ct);
                }

                return Negotiate
                    .WithModel(codeTypes)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetCodeTypesByUsageId(IRepositoryContextFactory repositoryContextFactory)
        {
            Get["GetCodeTypesByUsageId", "/types/{CodeUsageId:int}", true] = async (parameters, ct) =>
            {
                int codeUsageId = parameters.CodeUsageId;

                ICollection<CodeType> codeTypes;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    codeTypes = await repository.Configuration.Code.GetCodeTypes(codeUsageId, ct);
                }

                return Negotiate
                    .WithModel(codeTypes)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetCodeTypes(IRepositoryContextFactory repositoryContextFactory)
        {
            Get["GetCodeTypes", "/types", true] = async (parameters, ct) =>
            {
                ICollection<CodeType> codeTypes;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    codeTypes = await repository.Configuration.Code.GetCodeTypes(ct);
                }

                return Negotiate
                    .WithModel(codeTypes)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetCodeUsages(IRepositoryContextFactory repositoryContextFactory)
        {
            Get["GetCodeUsages", "/usages", true] = async (parameters, ct) =>
            {
                ICollection<CodeUsage> codeUsages;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    codeUsages = await repository.Configuration.Code.GetCodeUsages(ct);
                }

                return Negotiate
                    .WithModel(codeUsages)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }
    }
}
