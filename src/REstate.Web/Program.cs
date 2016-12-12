namespace REstate.Web
{
    using Autofac;
    using Engine;
    using Engine.Connectors.Scheduler;
    using Engine.Repositories;
    using Engine.Services;
    using Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Nancy.Owin;
    using OwinJwtAndCookie;
    using Repositories.Core.Susanoo;
    using REstate.Auth;
    using REstate.Auth.Repositories.MSSQL;
    using Scheduler;
    using Scheduler.Consumer.Direct;
    using Scheduler.Repositories.MSSQL;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using Nancy.ModelBinding;
    using RabbitMQ.Client;
    using Engine.Connectors.RabbitMq;
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json")
                .Build();

            var config = new RootConfig();
            ConfigurationBinder.Bind(configuration, config);

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            var containerBuilder = new ContainerBuilder();

            var container = RegisterComponents(containerBuilder, config, logger);

            var host = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions { });
                    app.UseOwin(owin =>
                    {
                        if (config.REstateConfiguration.AuthenticationSettings.UseAuthentication)
                        {
                            owin((next) =>
                                new JwtAndCookieMiddleware(next, new Options
                                {
                                    Certificate = new X509Certificate2(Path.Combine(Directory.GetCurrentDirectory(), "REstate.pfx"), "development"),
                                    CookieName = config.REstateConfiguration.AuthenticationSettings.CookieName,
                                    CookiePath = "/",
                                    CookieHttpOnly = true,
                                    TokenLifeSpan = TimeSpan.FromMinutes(config.REstateConfiguration.AuthenticationSettings.TokenLifeSpanMinutes),
                                    ClaimsPrincipalResourceName = config.REstateConfiguration.AuthenticationSettings.ClaimsPrincipalResourceName,
                                    CreatePrincipal = (payload) => CreateClaimsPrincipalFromPayload(container.Resolve<StringSerializer>(), payload)
                                }).Invoke);
                        }

                        owin.UseNancy(o =>
                        {
                            o.Bootstrapper = config.REstateConfiguration.AuthenticationSettings.UseAuthentication
                                ? new PlatformJwtNancyBootstrapper(container)
                                : new PlatformNancyBootstrapper(container);
                        });
                    });

                })
                .Build();

            var chronoConsumer = container.Resolve<ChronoConsumer>();

            using(CancellationTokenSource chronoConsumerSource = new CancellationTokenSource())
            {
                //Start the Consumer thread and hold a reference to the task.
                var chronoTask = chronoConsumer.StartAsync(config.REstateConfiguration.AuthenticationSettings.SchedulerApiKey, chronoConsumerSource.Token);

                //Running web layer, blocking call.
                host.Run();

                //Request work to stop.
                chronoConsumerSource.Cancel();

                //Wait for work to finish before exiting.
                chronoTask.Wait();
            }


        }

        private static IContainer RegisterComponents(ContainerBuilder containerBuilder, RootConfig config, ILogger logger)
        {
            containerBuilder.RegisterInstance(new ConnectionFactory()
            {
                Uri = "amqp://restate:restate_is_alive_2@40.118.135.89:5672/"
            });

            containerBuilder.RegisterType<JsonOnlyStringDictionaryBinder>().As<IModelBinder>();
            containerBuilder.RegisterInstance(config.REstateConfiguration);
            containerBuilder.RegisterInstance(logger).As<ILogger>();
            containerBuilder.RegisterAdapter<ILogger, IPlatformLogger>((serilogLogger) => new SerilogLoggingAdapter(serilogLogger));
            containerBuilder.RegisterType<SimpleJsonSerializer>().As<StringSerializer>();
            containerBuilder.Register((ctx)
                    => new ChronoRepositoryFactory(ctx.Resolve<REstateConfiguration>().ConnectionStrings.SchedulerConnectionString))
                .As<IChronoRepositoryFactory>();
            containerBuilder.RegisterType<TriggerSchedulerFactory>();
            containerBuilder.RegisterType<DirectChronoTriggerConnectorFactory>().As<IConnectorFactory>();
            containerBuilder.RegisterType<RabbitMqConnectorFactory>().As<IConnectorFactory>();
            containerBuilder.Register((ctx)
                    => new DefaultConnectorFactoryResolver(ctx.Resolve<IEnumerable<IConnectorFactory>>()))
                .As<IConnectorFactoryResolver>();
            containerBuilder.Register((ctx)
                    => new RepositoryContextFactory(ctx.Resolve<REstateConfiguration>().ConnectionStrings.EngineConnectionString, ctx.Resolve<StringSerializer>(), ctx.Resolve<IPlatformLogger>()))
                .As<IRepositoryContextFactory>();
            containerBuilder.RegisterInstance(new DotGraphCartographer()).AsSelf();
            containerBuilder.RegisterType<REstateMachineFactory>().As<IStateMachineFactory>();
            containerBuilder.RegisterType<StateEngineFactory>();
            containerBuilder.RegisterType<DirectChronoConsumer>().As<ChronoConsumer>();
            containerBuilder.Register((ctx)
                    => new AuthRepositoryFactory(ctx.Resolve<REstateConfiguration>().ConnectionStrings.AuthConnectionString))
                .As<IAuthRepositoryFactory>();

            return containerBuilder.Build();
        }

        private static ClaimsPrincipal CreateClaimsPrincipalFromPayload(StringSerializer serializer, IDictionary<string, object> payload)
        {
            if (payload == null) return null;

            object jtiObj;
            if (!payload.TryGetValue("jti", out jtiObj)) return null;

            object apikeyObj;
            if (!payload.TryGetValue("apikey", out apikeyObj)) return null;

            object identityObj;
            if (!payload.TryGetValue("sub", out identityObj)) return null;

            var identity = identityObj as string;
            var apikey = apikeyObj as string;
            var jti = jtiObj as string;
            if (identity == null || apikey == null) return null;

            var claims = payload.ContainsKey("claims")
                ? (serializer.Deserialize<IEnumerable<string>>(payload["claims"].ToString()))
                    .Select(claim => new Claim("claim", claim))
                  ?? new Claim[0]
                : new Claim[0];

            claims = claims.Union(new[] { new Claim("apikey", apikey), new Claim("jti", jti) });

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new GenericIdentity(identity),
                    claims));

            return principal;
        }
    }
}