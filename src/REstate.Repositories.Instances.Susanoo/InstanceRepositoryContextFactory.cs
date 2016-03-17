using System.Linq;
using REstate.Platform;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Instances.Susanoo
{
    public class InstanceRepositoryContextFactory
        : IInstanceRepositoryContextFactory
    {
        private readonly REstateConfiguration _configuration;

        public InstanceRepositoryContextFactory(REstateConfiguration configuration)
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