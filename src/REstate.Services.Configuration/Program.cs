using Autofac;
using Microsoft.Owin.Hosting;
using REstate.Auth.Repositories;
using REstate.Client;
using REstate.Client.Chrono;
using REstate.Connectors.Chrono;
using REstate.Connectors.RoslynScripting;
using REstate.Connectors.Susanoo;
using REstate.Owin;
using REstate.Repositories.Auth.Susanoo;
using REstate.Repositories.Configuration;
using REstate.Repositories.Configuration.Susanoo;
using REstate.Stateless;
using REstate.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using AutofacSerilogIntegration;
using REstate.Logging;
using REstate.Logging.Serilog;
using Serilog;

namespace REstate.Services.Configuration
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = ConfigurationManager.AppSettings["REstate.Services.HostBindingAddress"];

            var config = new REstateConfiguration
            {
                EncryptionPassphrase = ConfigurationManager.AppSettings["REstate.Web.EncryptionPassphrase"],
                HmacPassphrase = ConfigurationManager.AppSettings["REstate.Web.HmacPassphrase"],
                EncryptionSaltBytes = new byte[] { 0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2 },
                HmacSaltBytes = new byte[] { 0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2 },
                ClaimsPrincipalResourceName = ConfigurationManager.AppSettings["REstate.Web.ClaimsPrincipalResourceName"],
                LoginAddress = ConfigurationManager.AppSettings["REstate.Web.LoginAddress"],
                ApiKeyAddress = ConfigurationManager.AppSettings["REstate.Web.ApiKeyAddress"],
                ServesStaticContent = true,
                ConfigurationDictionary = new Dictionary<string, string>
                {
                    { "REstate.Web.ChronoAddress", ConfigurationManager.AppSettings["REstate.Web.ChronoAddress"] }
                }
            };

            var container = BuildAndConfigureContainer(config);

            container.RegisterInstance(config);
            var kernel = container.Build();

            //Binding to implementation
            REstateBootstrapper.KernelLocator = () => kernel;
            Startup.Config = config;


            var logger = kernel.Resolve<ILogger>();
            using (WebApp.Start<Startup>(url))
            {
                logger.Information("Running at {hostBindingAddress}", url);
                Console.WriteLine("Press enter to exit");

                Console.ReadLine();

            }
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstateConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.RegisterAdapter<ILogger, IREstateLogger>(serilogLogger =>
                new SerilogLoggingAdapter(serilogLogger));

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", "REstate.Services.Configuration")
                    .WriteTo.LiterateConsole()
                    .CreateLogger());

            container.Register(context => new ConfigurationRoutePrefix("/configuration"));

            container.RegisterType<AuthRepositoryContextFactory>()
                .As<IAuthRepositoryContextFactory>();

            container.RegisterType<ConfigurationRepositoryContextFactory>()
                .As<IConfigurationRepositoryContextFactory>();

            container.Register(context => new REstateClientFactory(configuration.LoginAddress))
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