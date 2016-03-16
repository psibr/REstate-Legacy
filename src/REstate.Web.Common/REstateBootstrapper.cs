﻿using Autofac;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Cryptography;
using Nancy.Diagnostics;
using Nancy.EmbeddedContent.Conventions;
using Nancy.Owin;
using Nancy.Responses.Negotiation;
using System;
using System.Security.Claims;
using REstate.Logging;
using REstate.Platform;

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

        public string PassPhrase;
        public REstateConfiguration Configuration;
        public static Func<ILifetimeScope> KernelLocator = null;

        protected override void ApplicationStartup(ILifetimeScope kernel, IPipelines pipelines)
        {
            base.ApplicationStartup(kernel, pipelines);

            var logger = kernel.Resolve<IREstateLogger>();

            pipelines.EnableJwtStatelessAuthentication(
                ctx => ctx.GetOwinEnvironment()[Configuration.ClaimsPrincipalResourceName] as ClaimsPrincipal,
                CryptographyConfiguration,
                logger);

            pipelines.OnError += (ctx, ex) =>
            {
                logger.Error(ex, "Request exception encountered.");

                if (ex is ArgumentException)
                    return new Negotiator(ctx)
                        .WithStatusCode(400)
                        //.WithReasonPhrase(ex.Message) //this doesn't work for some reason here.
                        .WithModel(new {reasonPhrase = ex.Message})
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
                    new PassphraseKeyGenerator(Configuration.EncryptionPassphrase,
                        Convert.FromBase64String(Configuration.EncryptionSaltBase64), 1000)),
                new DefaultHmacProvider(
                    new PassphraseKeyGenerator(Configuration.HmacPassphrase, Convert.FromBase64String(Configuration.HmacSaltBase64), 1000)));

        protected override ILifetimeScope GetApplicationContainer()
        {
            var kernel = KernelLocator == null
                ? base.GetApplicationContainer()
                : KernelLocator();

            Configuration = kernel.Resolve<REstateConfiguration>();

            return kernel;
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