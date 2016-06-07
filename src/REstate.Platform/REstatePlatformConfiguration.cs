using System.Linq;
using Psibr.Platform;
using System.Collections.Generic;

namespace REstate.Platform
{
    public class REstatePlatformConfiguration
        : PlatformConfiguration, IPlatformConfiguration
    {
        public HttpServiceAddressConfiguration AuthHttpService =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "auth")).Value;

        public HttpServiceAddressConfiguration AdminHttpService =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "admin")).Value;

        public HttpServiceAddressConfiguration ChronoHttpService =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "chrono")).Value;

        public HttpServiceAddressConfiguration CoreHttpService =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "core")).Value;

        public ProcessingServiceConfiguration ChronoConsumerConfig =>
            ProcessingServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "chronoConsumer")).Value;

        public ServiceCredentials ServiceCredentials { get; set; }

        public Dictionary<string, List<string>> ConnectorDecoratorAssociations { get; set; }
    }
}