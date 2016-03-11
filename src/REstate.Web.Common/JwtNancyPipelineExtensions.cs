using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Cryptography;
using System;
using System.Linq;
using System.Security.Claims;
using REstate.Logging;

namespace REstate.Web
{
    public static class JwtNancyPipelineExtensions
    {

        public static void EnableJwtStatelessAuthentication(this IPipelines pipelines,
            Func<NancyContext, ClaimsPrincipal> principalLocator,
            CryptographyConfiguration crypto, IREstateLogger logger)
        {


            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(ctx =>
            {
                var user = principalLocator(ctx);

                var jtiString = user?.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

                if (jtiString == null) return ctx.CurrentUser;

                var apiKeyAndJti =
                    crypto.EncryptionProvider.Decrypt(user.Claims.First(c => c.Type == "apikey").Value);

                var jtiMatch = apiKeyAndJti.Substring(0, jtiString.Length);

                if (jtiMatch != jtiString)
                    return null;

                var apiKey = apiKeyAndJti.Substring(jtiString.Length, apiKeyAndJti.Length - jtiString.Length);
                ctx.CurrentUser = new User
                {
                    UserName = user.Identity.Name,
                    Claims = user.Claims.Where(c => c.Type == "claim").Select(c => c.Value).ToList(),
                    ApiKey = apiKey
                };

                logger.Debug("Authenticated principal: {principalName}.", ctx.CurrentUser.UserName);


                return ctx.CurrentUser;
            }));
        }
    }
}
