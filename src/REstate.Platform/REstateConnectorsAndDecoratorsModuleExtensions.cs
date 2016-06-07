using Autofac;
using REstate.Platform;

namespace REstate
{
    public static class REstateConnectorsAndDecoratorsModuleExtensions
    {
        public static void RegisterDecoratorsAndConnectors(this ContainerBuilder builder,
            REstatePlatformConfiguration configuration,
            params object[] connectorOptions)
        {
            builder.RegisterModule(new REstateConnectorsAndDecoratorsModule(configuration, connectorOptions));
        }
    }
}