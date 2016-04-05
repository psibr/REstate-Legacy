using Autofac;
using REstate.Repositories.Auth.Susanoo;
using REstate.Repositories.Configuration;
using REstate.Repositories.Configuration.Susanoo;
using REstate.Stateless;
using AutofacSerilogIntegration;
using Newtonsoft.Json;
using Psibr.Platform;
using Psibr.Platform.Logging.Serilog;
using Psibr.Platform.Repositories;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Service;
using REstate.Platform;
using REstate.Web.Configuration;
using Serilog;
using Topshelf;

namespace REstate.Services.Configuration
{
    class Program
    {
        const string ServiceName = "REstate.Services.Configuration";

        static void Main(string[] args)
        {
            var configString = PlatformConfiguration.LoadConfigurationFile("REstateConfig.json");

            var config = JsonConvert.DeserializeObject<REstatePlatformConfiguration>(configString);

            var kernel = BuildAndConfigureContainer(config).Build();
            PlatformNancyBootstrapper.KernelLocator = () => kernel;

            HostFactory.Run(host =>
            {
                host.UseSerilog(kernel.Resolve<ILogger>());
                host.Service<PlatformApiService>(svc =>
                {
                    svc.ConstructUsing(() => kernel.Resolve<PlatformApiService>());
                    svc.WhenStarted(service => service.Start());
                    svc.WhenStopped(service => service.Stop());
                });

                host.RunAsNetworkService();
                host.StartAutomatically();

                host.SetServiceName(ServiceName);
            });
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstatePlatformConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.Register(ctx => configuration)
                .As<IPlatformConfiguration, PlatformConfiguration, REstatePlatformConfiguration>();

            container.RegisterInstance(new ApiServiceConfiguration
            {
                HostBindingAddress = configuration.ConfigurationAddress.Binding
            });

            container.RegisterType<PlatformApiService>();

            container.RegisterModule<SerilogPlatformLoggingModule>();

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", ServiceName)
                    .WriteTo.LiterateConsole()
                    .CreateLogger());

            container.Register(context => new ConfigurationRoutePrefix(string.Empty));

            container.RegisterType<AuthRepositoryContextFactory>()
                .As<IAuthRepositoryContextFactory>();

            container.RegisterType<ConfigurationRepositoryContextFactory>()
                .As<IConfigurationRepositoryContextFactory>();

            container.RegisterConnectors(configuration);

            container.RegisterType<DefaultConnectorFactoryResolver>()
                .As<IConnectorFactoryResolver>();

            container.RegisterType<StatelessStateMachineFactory>()
                .As<IStateMachineFactory>();

            return container;
        }
    }
}