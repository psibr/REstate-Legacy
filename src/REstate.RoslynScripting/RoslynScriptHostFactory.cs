using System.Threading.Tasks;
using REstate.Services;

namespace REstate.RoslynScripting
{
    public class RoslynScriptHostFactory
        : IScriptHostFactory
    {
        public Task<IScriptHost> BuildScriptHost(string apiKey)
        {
            return Task.FromResult<IScriptHost>(new RoslynScriptHost(apiKey));
        }
    }
}