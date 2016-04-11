using System.Linq;
using Psibr.Platform;
using REstate.Repositories.Instances;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Core.Susanoo
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
                .Single(ea => ea.Tags.Contains("core"));

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