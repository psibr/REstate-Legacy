using Autofac;
using REstate.Client;
using REstate.Client.Chrono;
using REstate.Platform;
using REstate.Services;

namespace REstate.Connectors.Chrono
{
    public class ChronoTriggerConnectorModule : Module, IREstateConnectorModule
    {
        public REstatePlatformConfiguration Configuration { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<ChronoTriggerConnectorFactory>()
                .As<IConnectorFactory>();

            builder.Register(context => new REstateClientFactory(Configuration.AuthHttpService.Address + "apikey"))
                .As<IREstateClientFactory>();

            builder.Register(context => context.Resolve<IREstateClientFactory>()
                .GetChronoClient(Configuration.ChronoHttpService.Address))
                .As<IAuthSessionClient<IChronoSession>>();
        }
    }
}