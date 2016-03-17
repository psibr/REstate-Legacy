using Autofac;
using REstate.Chrono;
using REstate.Client;
using REstate.Repositories.Chrono.Susanoo;
using System.Configuration;
using AutofacSerilogIntegration;
using Newtonsoft.Json;
using REstate.Logging.Serilog;
using REstate.Platform;
using Serilog;
using Topshelf;

namespace REstate.Services.ChronoConsumer
{
    class Program
    {
        const string ServiceName = "REstate.Services.ChronoConsumer";

        static void Main(string[] args)
        {
            var configString = REstateConfiguration.LoadConfigurationFile();

            var config = JsonConvert.DeserializeObject<REstateConfiguration>(configString);
            
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

                host.SetServiceName(ServiceName);
            });
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstateConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.RegisterInstance(configuration);

            container.RegisterInstance(new ConsumerServiceConfiguration
            {
                ApiKey = configuration.ChronoConsumerConfig.ApiKey
            });

            container.RegisterType<ChronoConsumerService>();

            container.RegisterModule<SerilogREstateLoggingModule>();

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", ServiceName)
                    .WriteTo.LiterateConsole()
                    .CreateLogger());

            container.RegisterType<ChronoRepositoryFactory>()
                .As<IChronoRepositoryFactory>();

            container.Register(context => context.Resolve<IChronoRepositoryFactory>().OpenRepository());

            container.Register(context => new REstateClientFactory(configuration.AuthAddress + "apikey"))
                .As<IREstateClientFactory>();

            container.Register(context => context.Resolve<IREstateClientFactory>()
                .GetInstancesClient(configuration.InstancesAddress.Address))
                .As<IAuthSessionClient<IInstancesSession>>();

            return container;
        }
    }

}
