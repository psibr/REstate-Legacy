using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace OwinJwtAndCookie
{
    public class Options
    {
        public Options()
        {
            CookieHttpOnly = true;
        }
        
        public string CookieName { get; set; }

        public string CookiePath { get; set; }

        public bool CookieHttpOnly { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public string ClaimsPrincipalResourceName { get; set; }

        public TimeSpan TokenLifeSpan { get; set; } = TimeSpan.FromMinutes(30);

        public Func<IDictionary<string, object>, ClaimsPrincipal> CreatePrincipal { get; set; }
    }
}