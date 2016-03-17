using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace REstate.Platform
{

    public class REstateConfiguration
    {
        public string EncryptionPassphrase { get; set; }

        public string HmacPassphrase { get; set; }

        public string HmacSaltBase64 { get; set; }

        public string ClaimsPrincipalResourceName { get; set; } = "REstate.Web.Principal";

        public string EncryptionSaltBase64 { get; set; }

        public HttpServiceAddressConfiguration AuthAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "auth")).Value;

        public HttpServiceAddressConfiguration AdminAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "admin")).Value;

        public HttpServiceAddressConfiguration ChronoAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "chrono")).Value;

        public HttpServiceAddressConfiguration InstancesAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "instances")).Value;

        public HttpServiceAddressConfiguration ConfigurationAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "configuration")).Value;

        public ProcessingServiceConfiguration ChronoConsumerConfig =>
            ProcessingServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "chronoConsumer")).Value;

        public IDictionary<string, HttpServiceAddressConfiguration> HttpServices { get; set; }

        public IDictionary<string, ProcessingServiceConfiguration> ProcessingServices { get; set; }

        public string CookieName { get; set; } = "REstate";

        public string CookiePath { get; set; } = "/";

        public bool ServesStaticContent { get; set; } = false;

        public string StaticContentRootRoutePath { get; set; } = "/";

        public int TokenLifeSpan { get; set; } = 30;

        public IDictionary<string, string> ConnectorConfig { get; set; }

        public ConnectionConfiguration[] Connections { get; set; }

        public static string LoadConfigurationFile()
        {
            var binLocation = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent;

            var solutionRoot = binLocation?.Parent?.Parent?.Parent;

            var configFile = solutionRoot?.GetFiles("REstateConfig.json", SearchOption.TopDirectoryOnly).FirstOrDefault();

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