using Autofac;
using Microsoft.Owin.Hosting;
using REstate.Auth.Repositories;
using REstate.Owin;
using REstate.Repositories.Auth.Susanoo;
using REstate.Web;
using System;
using System.Configuration;
using AutofacSerilogIntegration;
using REstate.Logging;
using REstate.Logging.Serilog;
using REstate.Services.Common.Api;
using Serilog;
using Topshelf;

namespace REstate.Services.Auth
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new REstateConfiguration
            {
                ServiceName = "REstate.Services.Auth",
                HostBindingAddress = ConfigurationManager.AppSettings["REstate.Services.HostBindingAddress"],
                EncryptionPassphrase = ConfigurationManager.AppSettings["REstate.Web.EncryptionPassphrase"],
                HmacPassphrase = ConfigurationManager.AppSettings["REstate.Web.HmacPassphrase"],
                EncryptionSaltBytes = new byte[] { 0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2 },
                HmacSaltBytes = new byte[] { 0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2 },
                ClaimsPrincipalResourceName = ConfigurationManager.AppSettings["REstate.Web.ClaimsPrincipalResourceName"],
                LoginRedirectAddress = ConfigurationManager.AppSettings["REstate.Web.Auth.LoginRedirectAddress"],
                LoginAddress = ConfigurationManager.AppSettings["REstate.Web.LoginAddress"]
            };

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

                host.SetServiceName(config.ServiceName);
            });
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstateConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.RegisterInstance(configuration);

            container.RegisterType<REstateApiService<Startup>>();

            container.RegisterAdapter<ILogger, IREstateLogger>(serilogLogger =>
                new SerilogLoggingAdapter(serilogLogger));

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", configuration.ServiceName)
                    .WriteTo.LiterateConsole()
                    .CreateLogger());

            container.Register(context => new AuthRoutePrefix(string.Empty));

            container.RegisterType<AuthRepositoryContextFactory>()
                .As<IAuthRepositoryContextFactory>();

            return container;
        }
    }
}