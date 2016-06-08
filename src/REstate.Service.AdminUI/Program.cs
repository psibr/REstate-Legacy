using System;
using System.Collections.Generic;
using Autofac;
using AutofacSerilogIntegration;
using Nancy.Bootstrapper;
using Psibr.Platform;
using Psibr.Platform.Logging.Serilog;
using Psibr.Platform.Nancy;
using Psibr.Platform.Serialization;
using Psibr.Platform.Serialization.NewtonsoftJson;
using Psibr.Platform.Service.Nancy;
using Psibr.Platform.Service.Nancy.Jwt;
using REstate.Platform;
using Serilog;
using Serilog.Sinks.RollingFile;
using Topshelf;

namespace REstate.Services.AdminUI
{
    internal class Program
    {
        private const string ServiceName = "REstate.Services.AdminUI";

        private static void Main(string[] args)
        {
            var kernel = BuildContainer();

            var configLoader = kernel.Resolve<IConfigurationLoader<REstatePlatformConfiguration>>();

            //var config = configLoader.Load(new Dictionary<string, string>
            //{
            //    { "serverAddress", "http://devubuntu2:8500" },
            //    { "path", "Applications/Services/REstate/REstateConfig.json" }
            //}).Result;

            var config = configLoader.Load(new Dictionary<string, string>
            {
                { "path", "..\\..\\..\\..\\REstateConfig.json" }
            }).Result;

            kernel = ConfigureContainer(kernel, config);

            HostFactory.Run(host =>
            {
                host.UseSerilog(kernel.Resolve<ILogger>());
                host.Service<PlatformNancyApiServiceWithJwt<REstatePlatformConfiguration>>(svc =>
                {
                    svc.ConstructUsing(() => kernel.Resolve<PlatformNancyApiServiceWithJwt<REstatePlatformConfiguration>>());
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

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.Register(context => new NewtonsoftJsonSerializer())
                .As<IStringSerializer, IByteSerializer>();

            builder.RegisterType<FileConfigurationLoader<REstatePlatformConfiguration>>()
                .As<IConfigurationLoader<REstatePlatformConfiguration>>();

            return builder.Build();
        }

        private static IContainer ConfigureContainer(IContainer container, REstatePlatformConfiguration configuration)
        {
            var builder = new ContainerBuilder();

            builder.Register(ctx => configuration)
                .As<IPlatformConfiguration, PlatformConfiguration, REstatePlatformConfiguration>();

            builder.RegisterInstance(new ApiServiceConfiguration<REstatePlatformConfiguration>(
                configuration, configuration.AdminHttpService));

            builder.RegisterType<PlatformNancyBootstrapper>()
                .As<INancyBootstrapper>();

            builder.RegisterType<PlatformNancyApiServiceWithJwt<REstatePlatformConfiguration>>();

            builder.RegisterModule<SerilogPlatformLoggingModule>();

            builder.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", ServiceName)
                    .WriteTo.LiterateConsole()
                    .If(_ => configuration.LoggerConfigurations.ContainsKey("rollingFile")
                             && configuration.LoggerConfigurations["rollingFile"].ContainsKey("path"),
                        (loggerConfig) =>
                            loggerConfig.WriteTo
                                .RollingFile(
                                    $"{configuration.LoggerConfigurations["rollingFile"]["path"]}" +
                                    $"\\{ServiceName}\\{{Date}}.log"))
                    .If(_ => configuration.LoggerConfigurations.ContainsKey("seq"), loggerConfig =>
                        loggerConfig.WriteTo.Seq(configuration.LoggerConfigurations["seq"]["serverUrl"],
                            apiKey: configuration.LoggerConfigurations["seq"]["apiKey"]))
                    .CreateLogger());

            builder.Update(container);

            return container;
        }
    }
}
