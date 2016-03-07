using System.Collections.Generic;

namespace REstate.Web
{
    public class REstateConfiguration
    {
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

        public IDictionary<string, string> ConfigurationDictionary { get; set; }

    }
}