using System.Threading.Tasks;
using REstate.Services;

namespace REstate.Chrono
{
    public class ChronoTriggerScriptHostFactory
        : IScriptHostFactory
    {
        public Task<IScriptHost> BuildScriptHost(string apiKey) =>
            Task.FromResult<IScriptHost>(new ChronoTriggerScriptHost(apiKey));
    }
}