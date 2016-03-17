using System.Linq;
using REstate.Auth.Repositories;
using REstate.Platform;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Auth.Susanoo
{
    public class AuthRepositoryContextFactory
        : IAuthRepositoryContextFactory
    {
        private readonly REstateConfiguration _configuration;

        public AuthRepositoryContextFactory(REstateConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IAuthRepository OpenAuthRepositoryContext()
        {
            var connectionConfig = _configuration.Connections
                .Single(ea => ea.Tags.Contains("auth"));

            return new AuthRepository(new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionString(
                        connectionConfig.ConnectionString,
                        connectionConfig.ProviderName)));
        }
    }
}
