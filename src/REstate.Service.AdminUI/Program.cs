using System;
using Autofac;
using AutofacSerilogIntegration;
using Newtonsoft.Json;
using Psibr.Platform;
using Psibr.Platform.Logging.Serilog;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Service;
using REstate.Platform;
using Serilog;
using Topshelf;

namespace REstate.Services.AdminUI
{
    class Program
    {
        const string ServiceName = "REstate.Services.AdminUI";

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
                configuration, configuration.AdminHttpService));

            container.RegisterType<PlatformApiService<REstatePlatformConfiguration>>();

            container.RegisterModule<SerilogPlatformLoggingModule>();

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", ServiceName)
                    .WriteTo.LiterateConsole()
                    .WriteTo.RollingFile($"{configuration.RollingFileLoggerPath}\\{ServiceName}\\{{Date}}.log")
                    .CreateLogger());

            return container;
        }
    }
}
