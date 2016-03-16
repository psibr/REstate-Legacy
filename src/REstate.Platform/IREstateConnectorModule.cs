using Autofac.Core;

namespace REstate.Platform
{
    public interface IREstateConnectorModule : IModule
    {
        REstateConfiguration Configuration { get; set; }
    }
}