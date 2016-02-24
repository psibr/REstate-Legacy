using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Services
{
    public class DefaultScriptHostFactoryResolver
        : IScriptHostFactoryResolver
    {
        private readonly IDictionary<int, IScriptHostFactory> _scriptHostFactories;

        public DefaultScriptHostFactoryResolver()
        {
            _scriptHostFactories = new Dictionary<int, IScriptHostFactory>();
        }

        public DefaultScriptHostFactoryResolver(IDictionary<int, IScriptHostFactory> scriptHostFactories)
        {
            _scriptHostFactories = new Dictionary<int, IScriptHostFactory>(0)
                .Union(scriptHostFactories)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public IScriptHostFactory ResolveScriptHostFactory(int codeType)
        {
            IScriptHostFactory scriptHostFactory;
            if(!_scriptHostFactories.TryGetValue(codeType, out scriptHostFactory))
                throw new ArgumentException("No ScriptHostFactory exists matching code type.", nameof(codeType));

            return scriptHostFactory;
        }
    }
}
