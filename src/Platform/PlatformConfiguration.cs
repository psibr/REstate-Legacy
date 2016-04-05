using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Platform
{
    public class PlatformConfiguration : IPlatformConfiguration
    {
        public string EncryptionPassphrase { get; set; }

        public string HmacPassphrase { get; set; }

        public string HmacSaltBase64 { get; set; }

        public string ClaimsPrincipalResourceName { get; set; } = "Principal";

        public string EncryptionSaltBase64 { get; set; }

        public IDictionary<string, HttpServiceAddressConfiguration> HttpServices { get; set; }

        public IDictionary<string, ProcessingServiceConfiguration> ProcessingServices { get; set; }

        public string CookieName { get; set; } = "PlatformCookie";

        public string CookiePath { get; set; } = "/";

        public bool ServesStaticContent { get; set; } = false;

        public string StaticContentRootRoutePath { get; set; } = "/";

        public int TokenLifeSpan { get; set; } = 30;

        public IDictionary<string, string> ConnectorConfig { get; set; }

        public ConnectionConfiguration[] Connections { get; set; }

        public static string LoadConfigurationFile(string fileName, DirectoryInfo location = null)
        {
            var binLocation = location ?? new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent;

            var solutionRoot = binLocation?.Parent?.Parent?.Parent;

            var configFile = solutionRoot?.GetFiles(fileName, SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (configFile == null) throw new Exception("no config");

            string configString;
            using (var stream = configFile.OpenText())
            {
                configString = stream.ReadToEnd();
            }

            return configString;
        }
    }
}