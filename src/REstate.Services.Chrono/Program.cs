using Autofac;
using Microsoft.Owin.Hosting;
using REstate.Chrono;
using REstate.Owin;
using REstate.Repositories.Chrono.Susanoo;
using REstate.Web;
using REstate.Web.Chrono;
using System;
using System.Configuration;
using AutofacSerilogIntegration;
using REstate.Logging;
using REstate.Logging.Serilog;
using Serilog;

namespace REstate.Services.Chrono
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = ConfigurationManager.AppSettings["REstate.Services.HostBindingAddress"];



            var config = new REstateConfiguration
            {
                EncryptionPassphrase = ConfigurationManager.AppSettings["REstate.Web.EncryptionPassphrase"],
                HmacPassphrase = ConfigurationManager.AppSettings["REstate.Web.HmacPassphrase"],
                EncryptionSaltBytes = new byte[] { 0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2 },
                HmacSaltBytes = new byte[] { 0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2 },
                ClaimsPrincipalResourceName = ConfigurationManager.AppSettings["REstate.Web.ClaimsPrincipalResourceName"],
                LoginAddress = ConfigurationManager.AppSettings["REstate.Web.LoginAddress"],
            };

            var container = BuildAndConfigureContainer(config);

            container.RegisterInstance(config);
            var kernel = container.Build();

            //Binding to implementation
            REstateBootstrapper.KernelLocator = () => kernel;
            Startup.Config = config;

            var logger = kernel.Resolve<ILogger>();
            using (WebApp.Start<Startup>(url))
            {
                logger.Information("Running at {hostBindingAddress}", url);
                Console.WriteLine("Press enter to exit");

                Console.ReadLine();

            }
        }

        private static ContainerBuilder BuildAndConfigureContainer(REstateConfiguration configuration)
        {
            var container = new ContainerBuilder();

            container.RegisterAdapter<ILogger, IREstateLogger>(serilogLogger =>
                new SerilogLoggingAdapter(serilogLogger));

            container.RegisterLogger(
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .Enrich.WithProperty("source", "REstate.Services.Chrono")
                    .WriteTo.LiterateConsole()
                    .CreateLogger());

            container.Register(context => new ChronoRoutePrefix(string.Empty));

            container.RegisterType<ChronoRepositoryFactory>()
                .As<IChronoRepositoryFactory>();

            container.Register(context => context.Resolve<IChronoRepositoryFactory>().OpenRepository());


            return container;
        }
    }
}