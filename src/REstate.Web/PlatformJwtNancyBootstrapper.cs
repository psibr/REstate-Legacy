using Autofac;
using Nancy.Bootstrapper;
using Nancy.Owin;
using System.Security.Claims;

namespace REstate.Web
{
    public class PlatformJwtNancyBootstrapper : PlatformNancyBootstrapper
    {
        protected override void ConfigureAuthentication(IPipelines pipelines)
        {
            pipelines.EnableJwtStatelessAuthentication(
                ctx => ctx
                    .GetOwinEnvironment()
                    .ContainsKey(Configuration.AuthenticationSettings.ClaimsPrincipalResourceName)
                        ? ctx.GetOwinEnvironment()
                            [Configuration.AuthenticationSettings
                                .ClaimsPrincipalResourceName] as ClaimsPrincipal
                        : null,
                Logger);
        }

        public PlatformJwtNancyBootstrapper(ILifetimeScope scopedKernel)
            : base(scopedKernel)
        {
        }
    }
}
