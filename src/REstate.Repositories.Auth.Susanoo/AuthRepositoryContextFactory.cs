using System.Linq;
using Psibr.Platform;
using Psibr.Platform.Repositories;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Auth.Susanoo
{
    public class AuthRepositoryContextFactory
        : IAuthRepositoryContextFactory
    {
        private readonly IPlatformConfiguration _configuration;

        public AuthRepositoryContextFactory(IPlatformConfiguration configuration)
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
