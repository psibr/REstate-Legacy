using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Configuration.Susanoo
{
    public class ConfigurationRepositoryContextFactory
        : IConfigurationRepositoryContextFactory
    {

        public IConfigurationRepository OpenConfigurationRepositoryContext(string apiKey)
        {
            return new ConfigurationRepository(
                new DatabaseManagerPool(
                    CommandManager.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionStringName("REstate")),
                apiKey);
        }
    }
}
