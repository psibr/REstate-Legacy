using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Instances.Susanoo
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