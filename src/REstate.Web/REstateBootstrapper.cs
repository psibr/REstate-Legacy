using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Cryptography;
using Nancy.Diagnostics;
using Nancy.EmbeddedContent.Conventions;
using Nancy.Owin;
using Nancy.Responses.Negotiation;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using REstate.Chrono;
using REstate.Chrono.Susanoo;
using REstate.Repositories;
using REstate.RoslynScripting;
using REstate.Services;
using REstate.Stateless;
using REstate.Susanoo;

namespace REstate.Web
{
    /// <summary>
    /// The bootstrapper enables you to reconfigure the composition of the framework,
    /// by overriding the various methods and properties.
    /// For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper
    /// </summary>
    public class REstateBootstrapper 
        : DefaultNancyBootstrapper
    {

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            container.Register<IConfigurationRepositoryContextFactory>(
                (cContainer, overloads) => new ConfigurationRepositoryContextFactory());

            container.Register<IInstanceRepositoryContextFactory>(
                (cContainer, overloads) => new InstanceRepositoryContextFactory());

            container.Register<IJsonSerializer>(
                (cContainer, overloads) => new NewtonsoftJsonSerializer());

            var engine = new ChronoEngineFactory().CreateEngine();
            container.Register(engine);

            container.Register<IScriptHostFactoryResolver>((cContainer, overloads) =>
                new DefaultScriptHostFactoryResolver(new Dictionary<int, IScriptHostFactory>
                {
                    { 1, new SusanooScriptHostFactory() },
                    { 2, new SusanooScriptHostFactory() },
                    { 3, new RoslynScriptHostFactory() },
                    { 4, new RoslynScriptHostFactory() },
                    { 5, new ChronoTriggerScriptHostFactory(
                        container.Resolve<IChronoEngine>(),
                        container.Resolve<IJsonSerializer>()) }
                }));

            container.Register<IStateMachineFactory>((cContainer, overloads) =>
                new StatelessStateMachineFactory(container.Resolve<IConfigurationRepositoryContextFactory>(),
                    container.Resolve<IInstanceRepositoryContextFactory>(),
                    container.Resolve<IScriptHostFactoryResolver>()));

            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                var owin = ctx.GetOwinEnvironment();

                string passPhrase = owin["REstate.passphrase"] as string;

                ClaimsPrincipal user = owin["server.User"] as ClaimsPrincipal;

                var jtiString = user?.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
                if (jtiString != null)
                {

                    var jti = Guid.Parse(jtiString);

                    ctx.CurrentUser = new User
                    {
                        UserName = user.Identity.Name,
                        Claims = user.Claims.Where(c => c.Type == "claim").Select(c => c.Value).ToList(),
                        ApiKey = new RijndaelEncryptionProvider(
                            new PassphraseKeyGenerator(passPhrase, jti.ToByteArray()))
                            .Decrypt(user.Claims.First(c => c.Type == "apikey").Value)
                    };
                }

                return ctx.CurrentUser;
            }));

            pipelines.OnError += (ctx, ex) =>
            {
                if (ex is ArgumentException)
                    return new Negotiator(ctx)
                        .WithStatusCode(400)
                        //.WithReasonPhrase(ex.Message) //this doesn't work for some reason here.
                        .WithModel(new { reasonPhrase = ex.Message })
                        .WithAllowedMediaRange(new MediaRange("application/json"));

                return null;
            };
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.StaticContentsConventions
                .AddEmbeddedDirectory("/Views");

            StaticConfiguration.DisableErrorTraces = false;
        }

        /// <summary>
        /// Gets the diagnostics / dashboard configuration (password etc)
        /// </summary>
        /// <value>The diagnostics configuration.</value>
        protected override DiagnosticsConfiguration DiagnosticsConfiguration { get; } = new DiagnosticsConfiguration
        {
            Password = "pass"
        };
    }

    public class NewtonsoftJsonSerializer
        : IJsonSerializer
    {
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload);
        }
    }
}