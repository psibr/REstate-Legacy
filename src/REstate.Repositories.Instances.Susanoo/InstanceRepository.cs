using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Instances.Susanoo
{
    public class InstanceRepository
        : IInstanceRepository
    {
        public InstanceRepository(IDatabaseManagerPool databaseManagerPool, string apiKey)
        {
            DatabaseManagerPool = databaseManagerPool;
            ApiKey = apiKey;
            MachineInstances = new MachineInstancesRepository(this);
        }

        public IInstanceRepository Root => this;

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

        public IMachineInstancesRepository MachineInstances { get; }
    }
}