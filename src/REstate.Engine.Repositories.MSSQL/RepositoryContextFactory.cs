using REstate.Engine.Repositories;
using Susanoo;
using Susanoo.ConnectionPooling;

namespace REstate.Repositories.Core.Susanoo
{
    public class RepositoryContextFactory
        : IRepositoryContextFactory
    {
        private readonly StringSerializer _StringSerializer;
        private readonly string _ConnectionString;

        public RepositoryContextFactory(string connectionString, StringSerializer stringSerializer)
        {
            _StringSerializer = stringSerializer;
            _ConnectionString = connectionString;
        }

        public IEngineRepositoryContext OpenContext(string apiKey)
        {
            return new EngineRepositoryContext(
                new DatabaseManagerPool(
                SusanooCommander.ResolveDatabaseManagerFactory(),
                factory => factory.CreateFromConnectionString(
                    System.Data.SqlClient.SqlClientFactory.Instance,
                    _ConnectionString)),
                _StringSerializer, apiKey);
        }
    }
}
