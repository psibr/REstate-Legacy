using System.Threading;
using System.Threading.Tasks;

namespace REstateClient
{
    public interface IAuthSessionClient<TSession>
        where TSession : IAuthenticatedSession
    {
        Task<TSession> GetSessionAsync(string apiKey, CancellationToken cancellationToken);
    }
}
