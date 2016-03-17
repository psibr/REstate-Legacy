using System.Linq;
using REstate.Chrono;
using REstate.Platform;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Chrono.Susanoo
{
    public class ChronoRepositoryFactory : IChronoRepositoryFactory
    {
        private readonly REstateConfiguration _configuration;

        public ChronoRepositoryFactory(REstateConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IChronoRepository OpenRepository()
        {
            var connectionConfig = _configuration.Connections
                .Single(ea => ea.Tags.Contains("chrono"));

            return new ChronoRepository(
                new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionString(
                        connectionConfig.ConnectionString,
                        connectionConfig.ProviderName)));
        }
    }
}
