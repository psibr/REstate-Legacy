using REstate.Logging;

namespace REstate.Engine.Repositories.InMemory
{
    public class EngineRepositoryContext
        : IEngineRepositoryContext
    {
        public EngineRepositoryContext(StringSerializer stringSerializer, IPlatformLogger logger,  string apiKey)
        {
            ApiKey = apiKey;

            var repo = new MachineConfigurationRepository(this, stringSerializer);

            Machines = repo;
            MachineInstances = repo;
        }

        public IEngineRepositoryContext Root => this;

        /// <summary>
        /// Gets the API key.
        /// </summary>
        /// <value>The API key.</value>
        public string ApiKey { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        public IMachineConfigurationRepository Machines { get; }

        public IMachineInstancesRepository MachineInstances { get; }
    }
}
