using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Auth.Repositories
{
    public interface IAuthRepository
        : IDisposable
    {
        Task<IPrincipal> LoadPrincipalByApiKey(string apiKey, CancellationToken cancellationToken);

        Task<IPrincipal> LoadPrincipalByCredentials(string username, string passwordHash, CancellationToken cancellationToken);
    }
}