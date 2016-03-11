using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace REstate.Owin
{
    public class JwtAndCookieMiddlewareOptions
    {
        public JwtAndCookieMiddlewareOptions()
        {
            CookieHttpOnly = true;
        }

        public string PassPhrase { get; set; }

        public string CookieName { get; set; }

        public string CookiePath { get; set; }

        public bool CookieHttpOnly { get; set; }

        public string ClaimsPrincipalResourceName { get; set; }

        public TimeSpan TokenLifeSpan { get; set; } = TimeSpan.FromMinutes(30);

        public Func<IDictionary<string, object>, ClaimsPrincipal> CreatePrincipal { get; set; }
    }
}