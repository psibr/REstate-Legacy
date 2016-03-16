using Autofac;
using REstate.Platform;
using REstate.Services;

namespace REstate.Connectors.RoslynScripting
{
    public class RoslynConnectorModule : Module, IREstateConnectorModule
    {
        public REstateConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<RoslynConnectorFactory>()
                .As<IConnectorFactory>();
        }
    }
}