using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Auth
{
    public interface IAuthRepository : IDisposable
    {
        Task<IPrincipal> LoadPrincipalByApiKeyAsync(string apiKey, CancellationToken cancellationToken);
        Task<IPrincipal> LoadPrincipalByCredentialsAsync(string username, string passwordHash, CancellationToken cancellationToken);
    }
}