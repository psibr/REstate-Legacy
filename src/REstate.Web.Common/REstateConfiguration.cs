using System;
using System.Collections.Generic;

namespace REstate.Web
{
    public class REstateConfiguration
    {
        public string ServiceName { get; set; }

        public string HostBindingAddress { get; set; }

        public string EncryptionPassphrase { get; set; }

        public string HmacPassphrase { get; set; }

        public byte[] HmacSaltBytes { get; set; }

        public string ClaimsPrincipalResourceName { get; set; } = "REstate.Web.Principal";

        public byte[] EncryptionSaltBytes { get; set; }

        public string LoginRedirectAddress { get; set; }

        public string LoginAddress { get; set; }

        public string ApiKeyAddress { get; set; }

        public string CookieName { get; set; } = "REstate";

        public string CookiePath { get; set; } = "/";

        public bool ServesStaticContent { get; set; } = false;

        public string StaticContentRootRoutePath { get; set; } = "/";

        public TimeSpan TokenLifeSpan { get; set; } = TimeSpan.FromMinutes(30);

        public IDictionary<string, string> ConfigurationDictionary { get; set; }

    }
}