using REstate.Logging;

namespace REstate.Engine.Repositories.InMemory
{
    public class EngineRepositoryContext
        : IEngineRepositoryContext
    {
        public EngineRepositoryContext(StringSerializer stringSerializer, IPlatformLogger logger,  string apiKey)
        {
            ApiKey = apiKey;

            var repo = new EngineRepository(this, stringSerializer);

            Schematics = repo;
            Machines = repo;
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

        public ISchematicRepository Schematics { get; }

        public IMachineRepository Machines { get; }
    }
}
