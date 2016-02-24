using System.Threading;
using System.Threading.Tasks;
using REstate.Repositories;

namespace REstate.Auth.Repositories
{
    public interface IAuthRepository
        : IConfigurationContextualRepository
    {
        Task<IPrincipal> LoadPrincipalByApiKey(string apiKey, CancellationToken cancellationToken);

        Task<IPrincipal> LoadPrincipalByCredentials(string username, string passwordHash, CancellationToken cancellationToken);
    }
}