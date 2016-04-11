using System.Linq;
using Psibr.Platform;
using REstate.Repositories.Configuration;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Core.Susanoo
{
    public class ConfigurationRepositoryContextFactory
        : IConfigurationRepositoryContextFactory
    {
        private readonly IPlatformConfiguration _configuration;

        public ConfigurationRepositoryContextFactory(IPlatformConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfigurationRepository OpenConfigurationRepositoryContext(string apiKey)
        {
            var connectionConfig = _configuration.Connections
                .Single(ea => ea.Tags.Contains("core"));

            return new ConfigurationRepository(
                new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionString(
                        connectionConfig.ConnectionString,
                        connectionConfig.ProviderName)),
                apiKey);
        }
    }
}
