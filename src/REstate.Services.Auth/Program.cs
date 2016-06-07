using System;
using System.Collections.Generic;
using Autofac;
using REstate.Repositories.Auth.Susanoo;
using AutofacSerilogIntegration;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Psibr.Platform;
using Psibr.Platform.Logging.Serilog;
using Psibr.Platform.Repositories;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Service;
using Psibr.Platform.Serialization;
using Psibr.Platform.Serialization.NewtonsoftJson;
using REstate.Platform;
using REstate.Web.Auth;
using Serilog;
using Serilog.Sinks.RollingFile;
using Topshelf;

namespace REstate.Services.Auth
{
    internal class Program
    {
        private const string ServiceName = "REstate.Services.Auth";

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
                host.Service<PlatformApiService<REstatePlatformConfiguration>>(svc =>
                {
                    svc.ConstructUsing(() => kernel.Resolve<PlatformApiService<REstatePlatformConfiguration>>());
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

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.Register(context => new NewtonsoftJsonSerializer(prettify: true))
                .As<IStringSerializer, IByteSerializer>();

            builder.RegisterType<FileConfigurationLoader<REstatePlatformConfiguration>>()
                .As<IConfigurationLoader<REstatePlatformConfiguration>>();

            return builder.Build();
        }

        private static IContainer ConfigureContainer(IContainer container, REstatePlatformConfiguration configuration)
        {
            container.Update(builder =>
            {

                builder.Register(ctx => configuration)
                    .As<IPlatformConfiguration, PlatformConfiguration, REstatePlatformConfiguration>();

                builder.RegisterInstance(new ApiServiceConfiguration<REstatePlatformConfiguration>(
                    configuration, configuration.AuthHttpService));

                builder.RegisterType<PlatformNancyBootstrapper>()
                    .As<INancyBootstrapper>();

                builder.RegisterType<PlatformApiService<REstatePlatformConfiguration>>();

                builder.RegisterModule<SerilogPlatformLoggingModule>();

                builder.RegisterLogger(
                    new LoggerConfiguration().MinimumLevel.Verbose()
                        .Enrich.WithProperty("source", ServiceName)
                        .WriteTo.LiterateConsole()
                        .If((loggerConfig) => configuration.LoggerConfigurations.ContainsKey("rollingFile")
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

                builder.Register(context => new AuthRoutePrefix(string.Empty));

                builder.RegisterType<AuthRepositoryContextFactory>()
                    .As<IAuthRepositoryContextFactory>()
                    .SingleInstance();
            });

            return container;
        }
    }
}