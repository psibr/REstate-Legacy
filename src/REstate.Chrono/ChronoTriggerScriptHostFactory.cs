using System.Threading.Tasks;
using REstate.Services;

namespace REstate.Chrono
{
    public class ChronoTriggerScriptHostFactory
        : IScriptHostFactory
    {
        private readonly IChronoEngine _engine;
        private readonly IJsonSerializer _json;

        public ChronoTriggerScriptHostFactory(IChronoEngine engine, IJsonSerializer json)
        {
            _engine = engine;
            _json = json;
        }

        public Task<IScriptHost> BuildScriptHost(string apiKey) =>
            Task.FromResult<IScriptHost>(new ChronoTriggerScriptHost(_engine, _json, apiKey));
    }
}