using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
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
        public Func<IDictionary<string, object>, ClaimsPrincipal> CreatePrincipal { get; set; }
    }
}