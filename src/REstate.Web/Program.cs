namespace REstate.Web
{
    using Autofac;
    using Engine;
    using Engine.Connectors.Scheduler;
    using Engine.Repositories;
    using Engine.Services;
    using Engine.Stateless;
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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using Microsoft.AspNetCore.Hosting.Server;
    using Nancy.ModelBinding;

    public class RootConfig
    {
        public REstateConfiguration REstateConfiguration { get; set; }
    }

    public class REstateConfiguration
    {
        public ConnectionStrings ConnectionStrings { get; set; }

        public AuthenticationSettings AuthenticationSettings { get; set; }
    }

    public class AuthenticationSettings
    {
        public string ClaimsPrincipalResourceName { get; set; }

        public string SchedulerApiKey { get; set; }
    }

    public class ConnectionStrings
    {
        public string EngineConnectionString { get; set; }

        private string _SchedulerConnectionString;
        public string SchedulerConnectionString
        {
            get
            {
                return _SchedulerConnectionString ?? EngineConnectionString;
            }
            set
            {
                _SchedulerConnectionString = value;
            }
        }

        private string _AuthConnectionString;
        public string AuthConnectionString
        {
            get
            {
                return _AuthConnectionString ?? EngineConnectionString;
            }
            set
            {
                _AuthConnectionString = value;
            }
        }

    }

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
            containerBuilder.Register((ctx) 
                    => new DefaultConnectorFactoryResolver(ctx.Resolve<IEnumerable<IConnectorFactory>>()))
                .As<IConnectorFactoryResolver>();
            containerBuilder.Register((ctx) 
                    => new RepositoryContextFactory(ctx.Resolve<REstateConfiguration>().ConnectionStrings.EngineConnectionString, ctx.Resolve<StringSerializer>(), ctx.Resolve<IPlatformLogger>()))
                .As<IRepositoryContextFactory>();
            containerBuilder.RegisterType<StatelessStateMachineFactory>().As<IStateMachineFactory>();
            containerBuilder.RegisterType<StateEngineFactory>();
            containerBuilder.RegisterType<DirectChronoConsumer>().As<ChronoConsumer>();
            containerBuilder.Register((ctx) 
                    => new AuthRepositoryFactory(ctx.Resolve<REstateConfiguration>().ConnectionStrings.AuthConnectionString))
                .As<IAuthRepositoryFactory>();

            var container = containerBuilder.Build();

            var host = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .Configure(app =>
                {
                    app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions { });
                    app.UseOwin(owin =>
                    {
                        owin((next) =>
                            new JwtAndCookieMiddleware(next, new Options
                            {
                                Certificate = new X509Certificate2(Path.Combine(Directory.GetCurrentDirectory(), "REstate.pfx"), "development"),
                                CookieName = "REstate.Auth",
                                CookiePath = "/",
                                CookieHttpOnly = true,
                                TokenLifeSpan = TimeSpan.FromMinutes(43200), //30 days
                                ClaimsPrincipalResourceName = config.REstateConfiguration.AuthenticationSettings.ClaimsPrincipalResourceName,
                                CreatePrincipal = (payload) => 
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
                                        ? (container.Resolve<StringSerializer>().Deserialize<IEnumerable<string>>(payload["claims"].ToString())).Select(claim => new Claim("claim", claim))
                                          ?? new Claim[0]
                                        : new Claim[0];

                                    claims = claims.Union(new[] { new Claim("apikey", apikey), new Claim("jti", jti) });

                                    var principal = new ClaimsPrincipal(
                                        new ClaimsIdentity(
                                            new GenericIdentity(identity),
                                            claims));

                                    return principal;
                                }
                            }).Invoke);

                        owin.UseNancy(o =>
                        {
                            o.Bootstrapper = new PlatformJwtNancyBootstrapper(container);
                        });


                        
                    });

                })
                .Build();

            var chronoConsumer = container.Resolve<ChronoConsumer>();

            //Start the Consumer thread and hold a reference to the task.
            var chronoTask = chronoConsumer.StartAsync(config.REstateConfiguration.AuthenticationSettings.SchedulerApiKey);

            //Running web layer, blocking call.
            host.Run();

            //Request work to stop.
            chronoConsumer.Stop();

            //Wait for work to finish before exiting.
            chronoTask.Wait();
        }
    }
}