using System;
using System.Security.Claims;
using Autofac;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Cryptography;
using Nancy.Diagnostics;
using Nancy.EmbeddedContent.Conventions;
using Nancy.Owin;
using Nancy.Responses.Negotiation;

namespace REstate.Web
{
    /// <summary>
    /// The bootstrapper enables you to reconfigure the composition of the framework,
    /// by overriding the various methods and properties.
    /// For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper
    /// </summary>
    public class REstateBootstrapper
        : AutofacNancyBootstrapper
    {

        public static string PassPhrase;

        public static string AuthBaseUrl;

        public static Func<ILifetimeScope> KernelLocator = null;
        public static string ClaimsPrincipalResourceName = "server.User";

        protected override void ApplicationStartup(ILifetimeScope kernel, IPipelines pipelines)
        {
            base.ApplicationStartup(kernel, pipelines);

            pipelines.EnableJwtStatelessAuthentication(
                ctx => ctx.GetOwinEnvironment()[ClaimsPrincipalResourceName] as ClaimsPrincipal,
                CryptographyConfiguration);

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

        protected override CryptographyConfiguration CryptographyConfiguration =>
            new CryptographyConfiguration(
                new RijndaelEncryptionProvider(
                    new PassphraseKeyGenerator(PassPhrase, new byte[]
                    {
                        0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2
                    }, 1000)),
                new DefaultHmacProvider(
                    new PassphraseKeyGenerator(PassPhrase, new byte[]
                    {
                        0x01, 0x02, 0xD1, 0xFF, 0x2F, 0x30, 0x1D, 0xF2
                    }, 1000)));

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
        protected override DiagnosticsConfiguration DiagnosticsConfiguration =>
            new DiagnosticsConfiguration
            {
                Password = "pass"
            };
    }
}