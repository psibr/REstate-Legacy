using Autofac;
using Psibr.Platform.Logging;
using REstate.Configuration;
using REstate.Platform;
using REstate.Services;

namespace REstate.Connectors.Decorators.Task
{
    public class TaskConnectorModule : Module, IREstateConnectorModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(ctx => new TaskConnectorDecorator(
                    ctx.Resolve<IPlatformLogger>(),
                    ctx.ResolveOptional<TaskConnectorOptions>()))
                .AsSelf()
                .Named<IConnectorDecorator>("REstate.Connectors.Decorators.Task")
                .As<IConnectorDecorator>();
        }

        public REstatePlatformConfiguration Configuration { get; set; }
    }
}
