using Autofac;
using REstate.Platform;
using REstate.Services;

namespace REstate.Connectors.Susanoo
{
    public class SusanooConnectorModule : Module, IREstateConnectorModule
    {
        public REstatePlatformConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<SusanooConnectorFactory>()
                .As<IConnectorFactory>();
        }
    }
}