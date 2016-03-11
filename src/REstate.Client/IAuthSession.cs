using System.Threading.Tasks;

namespace REstate.Client
{
    public interface IAuthSessionClient<TSession>
        where TSession : IAuthenticatedSession
    {
        Task<TSession> GetSession(string apiKey);
    }
}
