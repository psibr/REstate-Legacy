using System.Configuration;
using Autofac;
using AutofacSerilogIntegration;
using Newtonsoft.Json;
using REstate.ApiService;
using REstate.Logging.Serilog;
using REstate.Owin;
using REstate.Platform;
using REstate.Web;
using Serilog;
using Topshelf;

namespace REstate.Services.AdminUI
{
    class Program
    {
        const string ServiceName = "REstate.Services.AdminUI";

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

            return container;
        }
    }
}
