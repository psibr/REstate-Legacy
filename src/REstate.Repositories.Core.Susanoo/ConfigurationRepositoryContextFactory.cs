using System.Collections.Generic;
using System.Linq;
using Psibr.Platform;
using Psibr.Platform.Serialization;
using REstate.Repositories.Configuration;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Core.Susanoo
{
    public class ConfigurationRepositoryContextFactory
        : IConfigurationRepositoryContextFactory
    {
        private IStringSerializer StringSerializer { get; }
        private readonly IPlatformConfiguration _configuration;

        public ConfigurationRepositoryContextFactory(IPlatformConfiguration configuration, IStringSerializer stringSerializer)
        {
            StringSerializer = stringSerializer;
            _configuration = configuration;
        }

        public IConfigurationRepository OpenRepositoryContext(string apiKey, ICollection<string> claims)
        {
            var connectionConfig = _configuration.Connections
                .Single(ea => ea.Tags.Contains("core"));

            return new ConfigurationRepository(
                new DatabaseManagerPool(
                CommandManager.ResolveDatabaseManagerFactory(),
                factory => factory.CreateFromConnectionString(
                    connectionConfig.ConnectionString,
                    connectionConfig.ProviderName)),
                StringSerializer, apiKey);
        }

        public IConfigurationRepository OpenConfigurationRepositoryContext(string apiKey)
        {
            return OpenRepositoryContext(apiKey, null);
        }
    }
}
