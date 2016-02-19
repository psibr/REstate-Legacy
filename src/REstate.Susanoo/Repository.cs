using REstate.Repositories;

namespace REstate.Susanoo
{
    public class Repository
        : IRepository
    {
        public Repository(IDatabaseManagerPool databaseManagerPool, string apiKey)
        {
            DatabaseManagerPool = databaseManagerPool;
            ApiKey = apiKey;
            Configuration = new ConfigurationRepository(this);
            MachineFunctions = new MachineFunctionsRepository(this);
            MachineInstances = new MachineInstancesRepository(this);
        }

        public IRepository Root => this;

        /// <summary>
        /// Gets the API key.
        /// </summary>
        /// <value>The API key.</value>
        public string ApiKey { get; }

        /// <summary>
        /// Gets the database manager.
        /// </summary>
        /// <value>The database manager.</value>
        public IDatabaseManagerPool DatabaseManagerPool { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            DatabaseManagerPool.Dispose();
        }

        public IConfigurationRepository Configuration { get; }

        public IMachineFunctionsRepository MachineFunctions { get; }

        public IMachineInstancesRepository MachineInstances { get; }
    }
}