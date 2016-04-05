using Autofac.Core;

namespace REstate.Platform
{
    public interface IREstateConnectorModule : IModule
    {
        REstatePlatformConfiguration Configuration { get; set; }
    }
}