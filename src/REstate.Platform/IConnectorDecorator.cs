using REstate.Services;

namespace REstate.Platform
{
    public interface IConnectorDecorator
    {
        IConnector Decorate(IConnector connector);
    }
}