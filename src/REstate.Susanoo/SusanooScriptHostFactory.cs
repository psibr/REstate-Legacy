using System.Threading.Tasks;
using REstate.Services;

namespace REstate.Susanoo
{
    public class SusanooScriptHostFactory
        : IScriptHostFactory
    {
        public Task<IScriptHost> BuildScriptHost(string apiKey) => 
            Task.FromResult<IScriptHost>(new SusanooScriptHost(apiKey));
    }
}