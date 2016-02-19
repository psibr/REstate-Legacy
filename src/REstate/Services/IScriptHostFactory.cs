using System.Threading.Tasks;

namespace REstate.Services
{
    public interface IScriptHostFactory
    {
        Task<IScriptHost> BuildScriptHost();
    }
}