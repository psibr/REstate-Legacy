using System.Threading.Tasks;

namespace REstateClient
{
    public interface IAuthSessionClient<TSession>
        where TSession : IAuthenticatedSession
    {
        Task<TSession> GetSession(string apiKey);
    }
}
