using Psibr.Platform.Serialization;
using REstate.Repositories.Configuration;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Core.Susanoo
{
    public class ConfigurationRepository
        : IConfigurationRepository
    {
        public ConfigurationRepository(IDatabaseManagerPool databaseManagerPool, IStringSerializer stringSerializer, string apiKey)
        {
            DatabaseManagerPool = databaseManagerPool;
            ApiKey = apiKey;
            Machines = new MachineConfigurationRepository(this, stringSerializer);
            MachineInstances = new MachineInstancesRepository(this, stringSerializer);
        }

        public IConfigurationRepository Root => this;

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

        public IMachineConfigurationRepository Machines { get; }
        public IMachineInstancesRepository MachineInstances { get; }
    }
}