using REstate.Auth.Repositories;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Auth.Susanoo
{
    public class AuthRepositoryContextFactory
        : IAuthRepositoryContextFactory
    {
        public IAuthRepository OpenAuthRepositoryContext()
        {
            return new AuthRepository(new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionStringName("REstate")));
        }
    }
}
