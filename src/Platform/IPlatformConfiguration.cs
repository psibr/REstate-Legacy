using System.Collections.Generic;

namespace Platform
{
    public interface IPlatformConfiguration
    {
        string ClaimsPrincipalResourceName { get; set; }
        ConnectionConfiguration[] Connections { get; set; }
        IDictionary<string, string> ConnectorConfig { get; set; }
        string CookieName { get; set; }
        string CookiePath { get; set; }
        string EncryptionPassphrase { get; set; }
        string EncryptionSaltBase64 { get; set; }
        string HmacPassphrase { get; set; }
        string HmacSaltBase64 { get; set; }
        IDictionary<string, HttpServiceAddressConfiguration> HttpServices { get; set; }
        IDictionary<string, ProcessingServiceConfiguration> ProcessingServices { get; set; }
        bool ServesStaticContent { get; set; }
        string StaticContentRootRoutePath { get; set; }
        int TokenLifeSpan { get; set; }
    }
}