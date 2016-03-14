using Autofac;
using REstate.Client;
using REstate.Client.Chrono;
using REstate.Connectors.Chrono;
using REstate.Connectors.RoslynScripting;
using REstate.Connectors.Susanoo;
using REstate.Owin;
using REstate.Repositories.Configuration;
using REstate.Repositories.Configuration.Susanoo;
using REstate.Repositories.Instances;
using REstate.Repositories.Instances.Susanoo;
using REstate.Stateless;
using REstate.Web;
using System.Collections.Generic;
using System.Configuration;
using AutofacSerilogIntegration;
using REstate.Logging;
using REstate.Logging.Serilog;
using REstate.Services.Common.Api;
using Serilog;
using Topshelf;

namespace REstate.Services.Instances
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new REstateConfiguration
            {
                ServiceName = "REstate.Services.Instances",
                HostBindingAddress = ConfigurationManager.AppSettings["REstate.Services.HostBindingAddress"],
                EncryptionPassphrase = ConfigurationManager.AppSettings["REstate.Web.EncryptionPassphrase"],
                HmacPassphrase = ConfigurationManager.AppSettings["REstate.Web.HmacPassphrase"],
                EncryptionSaltBytes = new byte[] { 0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2 },
                HmacSaltBytes = new byte[] { 0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2 },
                ClaimsPrincipalResourceName = ConfigurationManager.AppSettings["REstate.Web.ClaimsPrincipalResourceName"],
                LoginAddress = ConfigurationManager.AppSettings["REstate.Web.LoginAddress"],
                ApiKeyAddress = ConfigurationManager.AppSettings["REstate.Web.ApiKeyAddress"],
                ConfigurationDictionary = new Dictionary<string, string>
                {
                    { "REstate.Web.ChronoAddress", ConfigurationManager.AppSettings["REstate.Web.ChronoAddress"] }
                }
            };


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

                host.SetServiceName(config.ServiceName);
            });
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstateConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.RegisterInstance(configuration);

            container.RegisterType<REstateApiService<Startup>>();

            container.RegisterAdapter<ILogger, IREstateLogger>(serilogLogger =>
                new SerilogLoggingAdapter(serilogLogger));

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", configuration.ServiceName)
                    .WriteTo.LiterateConsole()
                    .CreateLogger());

            container.Register(context => new InstancesRoutePrefix(string.Empty));

            container.RegisterType<ConfigurationRepositoryContextFactory>()
                .As<IConfigurationRepositoryContextFactory>();

            container.RegisterType<InstanceRepositoryContextFactory>()
                .As<IInstanceRepositoryContextFactory>();

            container.Register(context => new REstateClientFactory(configuration.ApiKeyAddress))
                .As<IREstateClientFactory>();

            container.Register(context => context.Resolve<IREstateClientFactory>()
                .GetChronoClient(configuration.ConfigurationDictionary["REstate.Web.ChronoAddress"]))
                .As<IAuthSessionClient<IChronoSession>>();

            container.RegisterType<RoslynConnectorFactory>()
                .As<IConnectorFactory>();
            container.RegisterType<SusanooConnectorFactory>()
                .As<IConnectorFactory>();
            container.RegisterType<ChronoTriggerConnectorFactory>()
                .As<IConnectorFactory>();

            container.RegisterType<DefaultConnectorFactoryResolver>()
                .As<IConnectorFactoryResolver>();

            container.RegisterType<StatelessStateMachineFactory>()
                .As<IStateMachineFactory>();

            return container;
        }
    }
}