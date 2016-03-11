namespace REstate.Services
{
    public interface IConnectorFactoryResolver
    {
        IConnectorFactory ResolveConnectorFactory(string connectorKey);
    }
}
