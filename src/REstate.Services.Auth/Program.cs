using System.Configuration;
using Autofac;
using REstate.Auth.Repositories;
using REstate.Owin;
using REstate.Repositories.Auth.Susanoo;
using REstate.Web;
using AutofacSerilogIntegration;
using Newtonsoft.Json;
using REstate.ApiService;
using REstate.Logging.Serilog;
using REstate.Platform;
using Serilog;
using Topshelf;

namespace REstate.Services.Auth
{
    class Program
    {
        const string ServiceName = "REstate.Services.Auth";

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

            container.Register(context => new AuthRoutePrefix(string.Empty));

            container.RegisterType<AuthRepositoryContextFactory>()
                .As<IAuthRepositoryContextFactory>();

            return container;
        }
    }
}