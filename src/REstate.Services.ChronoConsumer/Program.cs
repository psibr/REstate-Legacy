using Autofac;
using REstate.Chrono;
using REstate.Client;
using REstate.Repositories.Chrono.Susanoo;
using REstate.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using AutofacSerilogIntegration;
using REstate.Logging;
using REstate.Logging.Serilog;
using Serilog;

namespace REstate.Services.ChronoConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new REstateConfiguration
            {
                ApiKeyAddress = ConfigurationManager.AppSettings["REstate.Web.ApiKeyAddress"],
                ConfigurationDictionary = new Dictionary<string, string>
                {
                    { "REstate.Web.Chrono.InstancesAddress", ConfigurationManager.AppSettings["REstate.Web.Chrono.InstancesAddress"] }
                }
            };

            var container = BuildAndConfigureContainer(config);

            container.RegisterInstance(config);
            var kernel = container.Build();

            var logger = kernel.Resolve<ILogger>();

            var instanceClient = kernel.Resolve<IAuthSessionClient<IInstancesSession>>();
            using (var session = instanceClient
                .GetSession("98EC17D7-7F31-4A44-A911-6B4D10B3DC2E").Result)
            {
                logger.Information("Authenticated session acquired.");

                var engine = kernel.Resolve<IChronoRepository>();
                var consumer = new Repositories.Chrono.Susanoo.ChronoConsumer(engine, session);

                logger.Information("Starting ChronoConsumer.");

                consumer.Start();

                logger.Information("Watching or consuming ChronoTriggers.");
                Console.ReadLine();

                consumer.Stop();
            }
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstateConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.RegisterAdapter<ILogger, IREstateLogger>(serilogLogger =>
                new SerilogLoggingAdapter(serilogLogger));

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", "REstate.Services.Auth")
                    .WriteTo.LiterateConsole()
                    .CreateLogger());

            container.RegisterType<ChronoRepositoryFactory>()
                .As<IChronoRepositoryFactory>();

            container.Register(context => context.Resolve<IChronoRepositoryFactory>().OpenRepository());

            container.Register(context => new REstateClientFactory(configuration.ApiKeyAddress))
                .As<IREstateClientFactory>();

            container.Register(context => context.Resolve<IREstateClientFactory>()
                .GetInstancesClient(configuration.ConfigurationDictionary["REstate.Web.Chrono.InstancesAddress"]))
                .As<IAuthSessionClient<IInstancesSession>>();

            return container;
        }
    }

}
