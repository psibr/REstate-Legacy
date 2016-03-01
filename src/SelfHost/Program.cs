using System;
using System.Collections.Generic;
using System.Configuration;
using Autofac;
using Microsoft.Owin.Hosting;
using REstate.Auth.Repositories;
using REstate.Auth.Susanoo;
using REstate.Chrono;
using REstate.Chrono.Susanoo;
using REstate.Owin;
using REstate.Client;
using REstate.Repositories.Configuration;
using REstate.Repositories.Configuration.Susanoo;
using REstate.Repositories.Instances;
using REstate.Repositories.Instances.Susanoo;
using REstate.RoslynScripting;
using REstate.Services;
using REstate.Services.JsonNet;
using REstate.Stateless;
using REstate.Susanoo;
using REstate.Web;

namespace SelfHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var url = ConfigurationManager.AppSettings["REstate.url"];
            var passPhrase = ConfigurationManager.AppSettings["REstate.passphrase"];
            var authBaseUrl = ConfigurationManager.AppSettings["REstate.Web.Auth.Url"];

            REstateBootstrapper.AuthBaseUrl = authBaseUrl;

            Startup.PassPhrase = passPhrase;
            REstateBootstrapper.PassPhrase = passPhrase;

            var kernel = BuildAndConfigureContainer();
            REstateBootstrapper.KernelLocator = () => kernel;


            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");

                var client = new REstateClient("http://localhost/restate/");
                using (var session = client.GetAuthenticatedSession("98EC17D7-7F31-4A44-A911-6B4D10B3DC2E").Result)
                {
                    var engine = kernel.Resolve<IChronoEngine>();
                    var consumer = new ChronoConsumer(engine, session);

                    consumer.Start();

                    Console.ReadLine();

                    consumer.Stop();
                }
            }
        }

        private static IContainer BuildAndConfigureContainer()
        {
            var container = new ContainerBuilder();

            container.RegisterType<AuthRepositoryContextFactory>()
                .As<IAuthRepositoryContextFactory>();

            container.Register(context => new AuthRoutePrefix(string.Empty));
            container.Register(context => new ConfigurationRoutePrefix("/configuration"));
            container.Register(context => new InstancesRoutePrefix("/machines"));

            container.RegisterType<ConfigurationRepositoryContextFactory>()
                .As<IConfigurationRepositoryContextFactory>();

            container.RegisterType<InstanceRepositoryContextFactory>()
                .As<IInstanceRepositoryContextFactory>();

            container.RegisterType<NewtonsoftJsonSerializer>()
                .As<IJsonSerializer>();

            container.Register(context => new ChronoEngineFactory().CreateEngine())
                .SingleInstance();

            container.Register(context => new DefaultScriptHostFactoryResolver(new Dictionary<int, IScriptHostFactory>
            {
                {1, new SusanooScriptHostFactory()},
                {2, new SusanooScriptHostFactory()},
                {3, new RoslynScriptHostFactory()},
                {4, new RoslynScriptHostFactory()},
                {5, new ChronoTriggerScriptHostFactory(
                        context.Resolve<IChronoEngine>(),
                        context.Resolve<IJsonSerializer>())
                }
            })).As<IScriptHostFactoryResolver>();

            container.RegisterType<StatelessStateMachineFactory>()
                .As<IStateMachineFactory>();

            var kernel = container.Build();

            return kernel;
        }
    }
}