using System.Linq;
using REstate.Platform;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Configuration.Susanoo
{
    public class ConfigurationRepositoryContextFactory
        : IConfigurationRepositoryContextFactory
    {
        private readonly REstateConfiguration _configuration;

        public ConfigurationRepositoryContextFactory(REstateConfiguration configuration)
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
