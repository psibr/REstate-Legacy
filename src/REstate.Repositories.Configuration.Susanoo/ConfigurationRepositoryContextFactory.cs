using System.Linq;
using Psibr.Platform;
using REstate.Platform;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Configuration.Susanoo
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
                .Single(ea => ea.Tags.Contains("configuration"));

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
