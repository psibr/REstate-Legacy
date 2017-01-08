using REstate.Logging;
using Susanoo.ConnectionPooling;

namespace REstate.Engine.Repositories.MSSQL
{
    public class EngineRepositoryContext
        : IEngineRepositoryContext
    {
        public EngineRepositoryContext(IDatabaseManagerPool databaseManagerPool, StringSerializer stringSerializer, IPlatformLogger logger,  string apiKey)
        {
            DatabaseManagerPool = databaseManagerPool;
            ApiKey = apiKey;
            Schematics = new SchematicRepository(this, stringSerializer);
            Machines = new MachineRepository(this, stringSerializer, logger);
        }

        public IEngineRepositoryContext Root => this;

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

        public ISchematicRepository Schematics { get; }

        public IMachineRepository Machines { get; }
    }
}
