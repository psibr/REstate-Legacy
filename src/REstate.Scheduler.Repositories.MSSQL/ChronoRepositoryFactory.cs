using Susanoo.ConnectionPooling;

namespace REstate.Scheduler.Repositories.MSSQL
{
    public class ChronoRepositoryFactory : IChronoRepositoryFactory
    {
        private readonly string _ConnectionString;

        public ChronoRepositoryFactory(string connectionString)
        {
            _ConnectionString = connectionString;
        }

        public IChronoRepository OpenRepository(string apiKey)
        {
            return new ChronoRepository(
                new DatabaseManagerPool(
                    Susanoo.SusanooCommander.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionString(
                        System.Data.SqlClient.SqlClientFactory.Instance,
                        _ConnectionString)), apiKey);
        }
    }
}
