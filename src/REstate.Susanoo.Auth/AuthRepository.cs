using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Auth.Repositories;
using REstate.Susanoo;
using Susanoo;

namespace REstate.Auth.Susanoo
{

    public class AuthRepository
        : ConfigurationContextualRepository, IAuthRepository
    {

        public AuthRepository(ConfigurationRepository root)
            : base(root)
        {
        }

        public async Task<IPrincipal> LoadPrincipalByApiKey(string apiKey, CancellationToken cancellationToken)
        {
            var results = (await CommandManager.Instance
                .DefineCommand("SELECT PrincipalId, ApiKey, PrincipalType, UserOrApplicationName\n" +
                               "FROM Principals\n" +
                               "WHERE ApiKey = @ApiKey\n\n" +
                               "SELECT ClaimName FROM PrincipalClaims c\n" +
                               "INNER JOIN Principals p ON p.PrincipalId = c.PrincipalId\n" +
                               "WHERE p.ApiKey = @ApiKey", CommandType.Text)
                .DefineResults(typeof(Principal), typeof(string))
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { ApiKey = apiKey }, cancellationToken))
                .ToArray();

            var principal = results[0].Cast<IPrincipal>().SingleOrDefault();

            if (principal == null) return null;
            principal.Claims = results[1]?.Cast<string>().ToArray() ?? new string[0];

            return principal;
        }

        public async Task<IPrincipal> LoadPrincipalByCredentials(string username, string passwordHash, CancellationToken cancellationToken)
        {
            var results = (await CommandManager.Instance
                .DefineCommand("SELECT PrincipalId, ApiKey, PrincipalType, UserOrApplicationName\n" +
                               "FROM Principals\n" +
                               "WHERE UserOrApplicationName = @username AND PasswordHash = @passwordHash\n\n" +
                               "SELECT ClaimName FROM PrincipalClaims c\n" +
                               "INNER JOIN Principals p ON p.PrincipalId = c.PrincipalId\n" +
                               "WHERE p.UserOrApplicationName = @username AND p.PasswordHash = @passwordHash", CommandType.Text)
                .DefineResults(typeof(Principal), typeof(string))
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { username, passwordHash }, cancellationToken))
                .ToArray();

            var principal = results[0].Cast<IPrincipal>().SingleOrDefault();

            if (principal == null) return null;
            principal.Claims = results[1]?.Cast<string>().ToArray() ?? new string[0];

            return principal;
        }
    }
}
