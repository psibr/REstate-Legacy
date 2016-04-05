using System;
using Autofac;
using REstate.Repositories.Configuration;
using REstate.Repositories.Configuration.Susanoo;
using REstate.Repositories.Instances;
using REstate.Repositories.Instances.Susanoo;
using REstate.Stateless;
using AutofacSerilogIntegration;
using Newtonsoft.Json;
using Psibr.Platform;
using Psibr.Platform.Logging.Serilog;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Service;
using REstate.Platform;
using REstate.Web.Instances;
using Serilog;
using Topshelf;

namespace REstate.Services.Instances
{
    class Program
    {
        const string ServiceName = "REstate.Services.Instances";

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

            Console.ReadLine();
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstatePlatformConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.Register(ctx => configuration)
                .As<IPlatformConfiguration, PlatformConfiguration, REstatePlatformConfiguration>();

            container.RegisterInstance(new ApiServiceConfiguration
            {
                HostBindingAddress = configuration.InstancesAddress.Binding
            });

            container.RegisterType<PlatformApiService>();

            container.RegisterModule<SerilogPlatformLoggingModule>();

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", ServiceName)
                    .WriteTo.LiterateConsole()
                    .CreateLogger());

            container.Register(context => new InstancesRoutePrefix(string.Empty));

            container.RegisterType<ConfigurationRepositoryContextFactory>()
                .As<IConfigurationRepositoryContextFactory>();

            container.RegisterType<InstanceRepositoryContextFactory>()
                .As<IInstanceRepositoryContextFactory>();

            container.RegisterConnectors(configuration);

            container.RegisterType<DefaultConnectorFactoryResolver>()
                .As<IConnectorFactoryResolver>();

            container.RegisterType<StatelessStateMachineFactory>()
                .As<IStateMachineFactory>();

            return container;
        }
    }
}