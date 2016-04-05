using System.Linq;
using Psibr.Platform;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Instances.Susanoo
{
    public class InstanceRepositoryContextFactory
        : IInstanceRepositoryContextFactory
    {
        private readonly IPlatformConfiguration _configuration;

        public InstanceRepositoryContextFactory(IPlatformConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IInstanceRepository OpenInstanceRepositoryContext(string apiKey)
        {
            var connectionConfig = _configuration.Connections
                .Single(ea => ea.Tags.Contains("instances"));

            return new InstanceRepository(
                new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionString(
                        connectionConfig.ConnectionString,
                        connectionConfig.ProviderName)),
                apiKey);
        }
    }
}