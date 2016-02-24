using REstate.Repositories;
using Susanoo;

namespace REstate.Susanoo
{
    public class InstanceRepositoryContextFactory
        : IInstanceRepositoryContextFactory
    {

        public IInstanceRepository OpenInstanceRepositoryContext(string apiKey)
        {
            return new InstanceRepository(
                new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionStringName("REstate")),
                apiKey);
        }
    }
}