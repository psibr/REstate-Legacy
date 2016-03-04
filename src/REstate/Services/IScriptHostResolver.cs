namespace REstate.Services
{
    public interface IScriptHostFactoryResolver
    {
        IScriptHostFactory ResolveScriptHostFactory(int codeType);
    }
}
