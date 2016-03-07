using System.Threading.Tasks;
using REstate.Client;
using REstate.Client.Chrono;
using REstate.Services;

namespace REstate.Connectors.Chrono
{
    public class ChronoTriggerScriptHostFactory
        : IScriptHostFactory
    {
        private readonly IAuthSessionClient<IChronoSession> _client;

        public ChronoTriggerScriptHostFactory(IAuthSessionClient<IChronoSession> client)
        {
            _client = client;
        }

        public async Task<IScriptHost> BuildScriptHost(string apiKey) => 
            new ChronoTriggerScriptHost(await _client.GetSession(apiKey));
    }
}