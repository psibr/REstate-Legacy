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
using Topshelf;

namespace REstate.Services.ChronoConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new REstateConfiguration
            {
                ServiceName = "REstate.Services.ChronoConsumer",
                ApiKeyAddress = ConfigurationManager.AppSettings["REstate.Web.ApiKeyAddress"],
                ConfigurationDictionary = new Dictionary<string, string>
                {
                    { "REstate.Web.Chrono.InstancesAddress", ConfigurationManager.AppSettings["REstate.Web.Chrono.InstancesAddress"] },
                    { "ApiKey", "98EC17D7-7F31-4A44-A911-6B4D10B3DC2E" }
                }
            };
            
            var kernel = BuildAndConfigureContainer(config).Build();

            HostFactory.Run(host =>
            {
                host.UseSerilog(kernel.Resolve<ILogger>());
                host.Service<ChronoConsumerService>(svc =>
                {
                    svc.ConstructUsing(() => kernel.Resolve<ChronoConsumerService>());
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

            container.RegisterType<ChronoConsumerService>();

            container.RegisterAdapter<ILogger, IREstateLogger>(serilogLogger =>
                new SerilogLoggingAdapter(serilogLogger));

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", configuration.ServiceName)
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
