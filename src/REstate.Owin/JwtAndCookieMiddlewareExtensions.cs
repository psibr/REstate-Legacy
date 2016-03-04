using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;

namespace REstate.Owin
{
    public static class JwtAndCookieMiddlewareExtensions
    {

        public static IAppBuilder UseJwtAndCookieMiddleware(this IAppBuilder app, JwtAndCookieMiddlewareOptions options)
        {
            app.Use(new Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>((next) => 
                new JwtAndCookieMiddleware(next, options).Invoke));

            return app;
        }
    }
}