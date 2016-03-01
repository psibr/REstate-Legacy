using System;
using System.Security.Claims;
using Autofac;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.EmbeddedContent.Conventions;
using Nancy.Owin;
using Nancy.Responses.Negotiation;
using REstate.Web.Auth;

namespace REstate.Platform
{
    /// <summary>
    /// The bootstrapper enables you to reconfigure the composition of the framework,
    /// by overriding the various methods and properties.
    /// For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper
    /// </summary>
    public class REstateBootstrapper 
        : AutofacNancyBootstrapper
    {

        public static Func<ILifetimeScope> KernelLocator = null;
        public static string ClaimsPrinicpalResourceName = "server.User";
        public static string PassphraseResourceName = "REstate.passphrase";

        protected override void ApplicationStartup(ILifetimeScope kernel, IPipelines pipelines)
        {
            base.ApplicationStartup(kernel, pipelines);

            pipelines.EnableJwtStatelessAuthentication(
                ctx => ctx.GetOwinEnvironment()[ClaimsPrinicpalResourceName] as ClaimsPrincipal, 
                ctx => ctx.GetOwinEnvironment()[PassphraseResourceName] as string);

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

        protected override ILifetimeScope GetApplicationContainer()
        {
            return KernelLocator == null 
                ? base.GetApplicationContainer() 
                : KernelLocator();
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
}