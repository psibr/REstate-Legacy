namespace REstate.Platform
{
    using Autofac;
    using Services;
    using System.Collections.Generic;
    using System.Linq;

    public class REstateConnectorsAndDecoratorsModule
        : Module
    {
        protected REstatePlatformConfiguration Configuration { get; set; }
        protected object[] ConnectorOptions { get; set; }

        public REstateConnectorsAndDecoratorsModule(REstatePlatformConfiguration configuration,
            params object[] connectorOptions)
        {
            Configuration = configuration;
            ConnectorOptions = connectorOptions;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            //Register all provided connector options
            foreach (var options in ConnectorOptions)
            {
                builder.RegisterInstance(options)
                    .AsSelf();
            }

            //Register all connectors and decorators
            builder.RegisterConnectors(Configuration);

            //Register the connector <- decorators associations from Configuration
            builder.Register(context =>
            {
                var ctx = context.Resolve<IComponentContext>();

                return new ConnectorDecoratorAssociations(
                    Configuration.ConnectorDecoratorAssociations.Select(pair =>
                        new KeyValuePair<string, IEnumerable<IConnectorDecorator>>(
                            pair.Key,
                            pair.Value.Select(ctx.ResolveNamed<IConnectorDecorator>))));
            });

            //Register the Resolver
            builder.RegisterType<DecoratingConnectorFactoryResolver>()
                .UsingConstructor(() => new DecoratingConnectorFactoryResolver(
                    null, (ConnectorDecoratorAssociations)null))
                .As<IConnectorFactoryResolver>();

        }
    }
}