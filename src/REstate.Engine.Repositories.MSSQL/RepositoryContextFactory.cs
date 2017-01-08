using REstate.Logging;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Engine.Repositories.MSSQL
{
    public class RepositoryContextFactory
        : IRepositoryContextFactory
    {
        private readonly StringSerializer _StringSerializer;
        private readonly string _ConnectionString;
        private readonly IPlatformLogger _logger;

        public RepositoryContextFactory(string connectionString, StringSerializer stringSerializer, IPlatformLogger logger)
        {
            _StringSerializer = stringSerializer;
            _ConnectionString = connectionString;
            _logger = logger;
        }

        public IEngineRepositoryContext OpenContext(string apiKey)
        {
            return new EngineRepositoryContext(
                new DatabaseManagerPool(
                SusanooCommander.ResolveDatabaseManagerFactory(),
                factory => factory.CreateFromConnectionString(
                    System.Data.SqlClient.SqlClientFactory.Instance,
                    _ConnectionString)),
                _StringSerializer, _logger, apiKey);
        }
    }
}
