using Susanoo.ConnectionPooling;

namespace REstate.Auth.Repositories.MSSQL
{
    public class AuthRepositoryFactory : IAuthRepositoryFactory
    {
        private readonly string _ConnectionString;

        public AuthRepositoryFactory(string connectionString)
        {
            _ConnectionString = connectionString;
        }

        public IAuthRepository OpenRepository()
        {
            return new AuthRepository(
                new DatabaseManagerPool(
                    Susanoo.SusanooCommander.ResolveDatabaseManagerFactory(),
                    factory => factory.CreateFromConnectionString(
                        System.Data.SqlClient.SqlClientFactory.Instance,
                        _ConnectionString)));
        }
    }
}
