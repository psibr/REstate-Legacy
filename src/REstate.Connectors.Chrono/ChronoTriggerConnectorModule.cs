using Autofac;
using REstate.Client;
using REstate.Client.Chrono;
using REstate.Platform;
using REstate.Services;

namespace REstate.Connectors.Chrono
{
    public class ChronoTriggerConnectorModule : Module, IREstateConnectorModule
    {
        public REstateConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<ChronoTriggerConnectorFactory>()
                .As<IConnectorFactory>();

            builder.Register(context => new REstateClientFactory(Configuration.AuthAddress + "apikey"))
                .As<IREstateClientFactory>();

            builder.Register(context => context.Resolve<IREstateClientFactory>()
                .GetChronoClient(Configuration.ChronoAddress.Address))
                .As<IAuthSessionClient<IChronoSession>>();
        }
    }
}
