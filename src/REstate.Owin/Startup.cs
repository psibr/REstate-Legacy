using Owin;
using REstate.Web;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Nancy.Owin;
using Nancy.Routing;

namespace REstate.Owin
{
    public class Startup
    {
        public static REstateConfiguration Config;

        public void Configuration(IAppBuilder app)
        {
            app.UseJwtAndCookieMiddleware(new JwtAndCookieMiddlewareOptions
            {
                PassPhrase = Config.HmacPassphrase,
                CookieName = Config.CookieName,
                CookiePath = Config.CookiePath,
                CookieHttpOnly = true,
                TokenLifeSpan = Config.TokenLifeSpan,
                ClaimsPrincipalResourceName = Config.ClaimsPrincipalResourceName,
                CreatePrincipal = CreatePrincipal
            });

            var options = new NancyOptions();

            if (Config.ServesStaticContent)
                options.PerformPassThrough = context =>
                    context.ResolvedRoute.Description.Method == "GET"
                    && (context.ResolvedRoute is NotFoundRoute
                        || context.ResolvedRoute.Description.Path == Config.StaticContentRootRoutePath)
                    && context.GetOwinEnvironment().ContainsKey(Config.ClaimsPrincipalResourceName)
                    && context.GetOwinEnvironment()[Config.ClaimsPrincipalResourceName] != null;

            app.UseNancy(options);

            if (Config.ServesStaticContent)
                app.UseStatic("wwwroot");
        }

        private static ClaimsPrincipal CreatePrincipal(IDictionary<string, object> payload)
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
                ? (payload["claims"] as IEnumerable)?.Cast<string>().Select(claim => new Claim("claim", claim))
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