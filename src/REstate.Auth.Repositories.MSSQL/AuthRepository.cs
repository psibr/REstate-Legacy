using Susanoo.ConnectionPooling;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Susanoo.SusanooCommander;

namespace REstate.Auth.Repositories.MSSQL
{

    public class AuthRepository : IAuthRepository, IDisposable
    {

        protected IDatabaseManagerPool DatabaseManagerPool { get; }

        public AuthRepository(IDatabaseManagerPool databaseManagerPool)
        {
            DatabaseManagerPool = databaseManagerPool;
        }

        public async Task<IPrincipal> LoadPrincipalByApiKeyAsync(string apiKey, CancellationToken cancellationToken)
        {
            var results = (await DefineCommand(
                    "SELECT ApiKey, PrincipalType, UserOrApplicationName\n" +
                    "FROM Principals\n" +
                    "WHERE ApiKey = @ApiKey\n\n" +
                    "SELECT ClaimName FROM PrincipalClaims c\n" +
                    "INNER JOIN Principals p ON p.ApiKey = c.ApiKey\n" +
                    "WHERE p.ApiKey = @ApiKey")
                .WithResultsAs(typeof(Principal), typeof(string))
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { ApiKey = apiKey }, cancellationToken)
                    .ConfigureAwait(false))
                .ToArray();

            var principal = results[0].Cast<IPrincipal>().SingleOrDefault();

            if (principal == null) return null;
            principal.Claims = results[1]?.Cast<string>().ToArray() ?? new string[0];

            return principal;
        }

        public async Task<IPrincipal> LoadPrincipalByCredentialsAsync(string username, string passwordHash, CancellationToken cancellationToken)
        {
            var results = (await DefineCommand(
                    "SELECT ApiKey, PrincipalType, UserOrApplicationName\n" +
                    "FROM Principals\n" +
                    "WHERE UserOrApplicationName = @username AND PasswordHash = @passwordHash\n\n" +
                    "SELECT ClaimName FROM PrincipalClaims c\n" +
                    "INNER JOIN Principals p ON p.ApiKey = c.ApiKey\n" +
                    "WHERE p.UserOrApplicationName = @username AND p.PasswordHash = @passwordHash")
                .WithResultsAs(typeof(Principal), typeof(string))
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { username, passwordHash }, cancellationToken)
                    .ConfigureAwait(false))
                .ToArray();

            var principal = results[0].Cast<IPrincipal>().SingleOrDefault();

            if (principal == null) return null;
            principal.Claims = results[1]?.Cast<string>().ToArray() ?? new string[0];

            return principal;
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                DatabaseManagerPool.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~AuthRepository()
        {
            Dispose(false);
        }
    }
}
