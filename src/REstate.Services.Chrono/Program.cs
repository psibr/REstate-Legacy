﻿using System;
using Autofac;
using REstate.Chrono;
using REstate.Repositories.Chrono.Susanoo;
using REstate.Web.Chrono;
using AutofacSerilogIntegration;
using Nancy.Bootstrapper;
using Newtonsoft.Json;
using Psibr.Platform;
using Psibr.Platform.Logging.Serilog;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Service;
using REstate.Platform;
using Serilog;
using Topshelf;

namespace REstate.Services.Chrono
{
    class Program
    {
        const string ServiceName = "REstate.Services.Chrono";

        static void Main(string[] args)
        {
            var configString = PlatformConfiguration.LoadConfigurationFile("REstateConfig.json");

            var config = JsonConvert.DeserializeObject<REstatePlatformConfiguration>(configString);

            var kernel = BuildAndConfigureContainer(config).Build();

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
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstatePlatformConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.Register(ctx => configuration)
                .As<IPlatformConfiguration, PlatformConfiguration, REstatePlatformConfiguration>();

            container.RegisterInstance(new ApiServiceConfiguration<REstatePlatformConfiguration>(
                configuration, configuration.ChronoHttpService));

            container.RegisterType<PlatformNancyBootstrapper>()
                .As<INancyBootstrapper>();

            container.RegisterType<PlatformApiService<REstatePlatformConfiguration>>();

            container.RegisterModule<SerilogPlatformLoggingModule>();

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", ServiceName)
                    .WriteTo.LiterateConsole()
                    .If((loggerConfig) => configuration.LoggerConfigurations.ContainsKey("rollingFile")
                        && configuration.LoggerConfigurations["rollingFile"].ContainsKey("path"), (loggerConfig) =>
                           loggerConfig.WriteTo
                               .RollingFile($"{configuration.LoggerConfigurations["rollingFile"]["path"]}\\{ServiceName}\\{{Date}}.log"))
                    .If(_ => configuration.LoggerConfigurations.ContainsKey("seq"), loggerConfig =>
                        loggerConfig.WriteTo.Seq(configuration.LoggerConfigurations["seq"]["serverUrl"],
                            apiKey: configuration.LoggerConfigurations["seq"]["apiKey"]))
                    .CreateLogger());

            container.Register(context => new ChronoRoutePrefix(string.Empty));

            container.RegisterType<ChronoRepositoryFactory>()
                .As<IChronoRepositoryFactory>();

            container.Register(context => context.Resolve<IChronoRepositoryFactory>().OpenRepository());


            return container;
        }
    }
}