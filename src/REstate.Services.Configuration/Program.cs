using Autofac;
using REstate.Auth.Repositories;
using REstate.Owin;
using REstate.Repositories.Auth.Susanoo;
using REstate.Repositories.Configuration;
using REstate.Repositories.Configuration.Susanoo;
using REstate.Stateless;
using REstate.Web;
using System.Collections.Generic;
using System.Configuration;
using AutofacSerilogIntegration;
using Newtonsoft.Json;
using REstate.ApiService;
using REstate.Logging;
using REstate.Logging.Serilog;
using REstate.Platform;
using Serilog;
using Topshelf;

namespace REstate.Services.Configuration
{
    class Program
    {
        const string ServiceName = "REstate.Services.Configuration";

        static void Main(string[] args)
        {
            var configString = REstateConfiguration.LoadConfigurationFile();

            var config = JsonConvert.DeserializeObject<REstateConfiguration>(configString);

            Startup.Config = config;
            var kernel = BuildAndConfigureContainer(config).Build();
            REstateBootstrapper.KernelLocator = () => kernel;

            HostFactory.Run(host =>
            {
                host.UseSerilog(kernel.Resolve<ILogger>());
                host.Service<REstateApiService<Startup>>(svc =>
                {
                    svc.ConstructUsing(() => kernel.Resolve<REstateApiService<Startup>>());
                    svc.WhenStarted(service => service.Start());
                    svc.WhenStopped(service => service.Stop());
                });

                host.RunAsNetworkService();
                host.StartAutomatically();

                host.SetServiceName(ServiceName);
            });
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstateConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.RegisterInstance(configuration);

            container.RegisterInstance(new REstateApiServiceConfiguration
            {
                HostBindingAddress = ConfigurationManager.AppSettings["REstate.Services.HostBindingAddress"]
            });

            container.RegisterType<REstateApiService<Startup>>();

            container.RegisterModule<SerilogREstateLoggingModule>();

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