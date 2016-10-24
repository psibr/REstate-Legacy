using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using REstate.Logging;
using System;
using System.Linq;
using System.Security.Claims;

namespace REstate.Web
{
    public static class JwtNancyPipelineExtensions
    {
        public static void EnableJwtStatelessAuthentication(this IPipelines pipelines,
            Func<NancyContext, ClaimsPrincipal> principalLocator, IPlatformLogger logger)
        {
            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                var user = principalLocator(ctx);

                if (user != null)
                {
                    var identity = new ClaimsIdentity(user.Claims.Union(new[] { new Claim("ApiKey", user.Claims.First(c => c.Type == "apikey").Value) }), "jwt");

                    ctx.CurrentUser = new ClaimsPrincipal(identity);

                    logger
                        .ForContext("clientAddress", ctx.Request.UserHostAddress)
                        .Verbose("Authenticated principal: {principalName} " +
                                 "for request {requestMethod} {requestPath}.",
                                 ctx.CurrentUser.Identity.Name, ctx.Request.Method, ctx.Request.Path);
                }

                return ctx.CurrentUser;
            }));
        }
    }
}
