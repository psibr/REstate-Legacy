using System;
using Autofac;
using REstate.Repositories.Auth.Susanoo;
using AutofacSerilogIntegration;
using Newtonsoft.Json;
using Psibr.Platform;
using Psibr.Platform.Logging.Serilog;
using Psibr.Platform.Repositories;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Service;
using REstate.Platform;
using REstate.Web.Auth;
using Serilog;
using Topshelf;

namespace REstate.Services.Auth
{
    class Program
    {
        const string ServiceName = "REstate.Services.Auth";

        static void Main(string[] args)
        {
            var configString = PlatformConfiguration.LoadConfigurationFile("REstateConfig.json");

            var config = JsonConvert.DeserializeObject<REstatePlatformConfiguration>(configString);

            var kernel = BuildAndConfigureContainer(config).Build();
            PlatformNancyBootstrapper.KernelLocator = () => kernel;

            HostFactory.Run(host =>
            {
                host.UseSerilog(kernel.Resolve<ILogger>());
                host.Service<PlatformApiService>(svc =>
                {
                    svc.ConstructUsing(() => kernel.Resolve<PlatformApiService>());
                    svc.WhenStarted(service => 
                        service.Start());
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

            container.RegisterInstance(new ApiServiceConfiguration
            {
                HostBindingAddress = configuration.AuthAddress.Binding
            });

            container.RegisterType<PlatformApiService>();

            container.RegisterModule<SerilogPlatformLoggingModule>();

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", ServiceName)
                    .WriteTo.LiterateConsole()
                    .WriteTo.RollingFile($"..\\..\\..\\..\\logs\\{ServiceName}\\{{Date}}.log")
                    .CreateLogger());

            container.Register(context => new AuthRoutePrefix(string.Empty));

            container.RegisterType<AuthRepositoryContextFactory>()
                .As<IAuthRepositoryContextFactory>().SingleInstance();

            return container;
        }
    }
}