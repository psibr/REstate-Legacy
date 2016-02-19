using REstate.Repositories;
using Susanoo;

namespace REstate.Susanoo
{
    public class RepositoryContextFactory
        : IRepositoryContextFactory
    {

        public IRepository OpenRepositoryContext(string apiKey)
        {
            return new Repository(
                new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionStringName("REstate")),
                apiKey);
        }
    }
}
