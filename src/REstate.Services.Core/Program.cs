using System;
using Autofac;
using AutofacSerilogIntegration;
using Newtonsoft.Json;
using Psibr.Platform;
using Psibr.Platform.Logging;
using Psibr.Platform.Logging.Serilog;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Service;
using Psibr.Platform.Serialization;
using Psibr.Platform.Serialization.NewtonsoftJson;
using REstate.Platform;
using REstate.Repositories.Configuration;
using REstate.Repositories.Core.Susanoo;
using REstate.Repositories.Instances;
using REstate.Stateless;
using REstate.Web.Core;
using Serilog;
using Topshelf;

namespace REstate.Services.Core
{
    class Program
    {
        const string ServiceName = "REstate.Services.Core";

        static void Main(string[] args)
        {
            var configString = PlatformConfiguration.LoadConfigurationFile("REstateConfig.json");

            var config = JsonConvert.DeserializeObject<REstatePlatformConfiguration>(configString);

            var kernel = BuildAndConfigureContainer(config).Build();
            PlatformNancyBootstrapper.KernelLocator = () => kernel;

            HostFactory.Run(host =>
            {
                host.UseSerilog(kernel.Resolve<ILogger>());
                host.Service<PlatformApiService<REstatePlatformConfiguration>>(svc =>
                {
                    svc.ConstructUsing(() => kernel.Resolve<PlatformApiService<REstatePlatformConfiguration>>());
                    svc.WhenStarted(service => service.Start());
                    svc.WhenStopped(service => service.Stop());
                });

                if (config.ServiceCredentials.Username.Equals("NETWORK SERVICE", StringComparison.OrdinalIgnoreCase))
                    host.RunAsNetworkService();
                else
                    host.RunAs(config.ServiceCredentials.Username, config.ServiceCredentials.Password);

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

            container.RegisterInstance(new ApiServiceConfiguration<REstatePlatformConfiguration>(
                configuration, configuration.CoreHttpService));

            container.RegisterType<PlatformApiService<REstatePlatformConfiguration>>();

            container.RegisterModule<SerilogPlatformLoggingModule>();

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", ServiceName)
                    .WriteTo.LiterateConsole()
                    .WriteTo.RollingFile($"{configuration.RollingFileLoggerPath}\\{ServiceName}\\{{Date}}.log")
                    .CreateLogger());

            container.RegisterType<NewtonsoftJsonSerializer>()
                .As<IStringSerializer, IByteSerializer>();

            container.Register(context => new InstancesRoutePrefix("/machines"));
            container.Register(context => new ConfigurationRoutePrefix("/configuration"));

            container.RegisterType<ConfigurationRepositoryContextFactory>()
                .As<IConfigurationRepositoryContextFactory>();

            container.RegisterType<InstanceRepositoryContextFactory>()
                .As<IInstanceRepositoryContextFactory>();

            container.RegisterConnectors(configuration);

            container.RegisterType<DefaultConnectorFactoryResolver>()
                .As<IConnectorFactoryResolver>();

            container.RegisterType<StatelessStateMachineFactory>()
                .As<IStateMachineFactory>();

            container.Register<Func<IConnector, IPlatformLogger, IConnector>>(ctx => 
                ((connector, logger) =>
                    new TaskConnectorDecorator(connector, logger))).SingleInstance();

            return container;
        }
    }

    public static class FluentIfExtension
    {
        public static T If<T>(this T fluentObject, Func<T, bool> predicate, Action<T> fluentContinuation)
        {
            if (predicate(fluentObject))
            {
                fluentContinuation(fluentObject);
            }

            return fluentObject;
        }
    }
}