using REstate.Chrono;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Chrono.Susanoo
{
    public class ChronoRepositoryFactory : IChronoRepositoryFactory
    {
        public IChronoRepository OpenRepository()
        {
            return new ChronoRepository(
                new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionStringName("REstate")));
        }
    }
}
