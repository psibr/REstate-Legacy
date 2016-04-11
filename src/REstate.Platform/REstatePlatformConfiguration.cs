using System.Linq;
using Psibr.Platform;

namespace REstate.Platform
{
    public class REstatePlatformConfiguration
        : PlatformConfiguration, IPlatformConfiguration
    {
        public HttpServiceAddressConfiguration AuthAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "auth")).Value;

        public HttpServiceAddressConfiguration AdminAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "admin")).Value;

        public HttpServiceAddressConfiguration ChronoAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "chrono")).Value;

        public HttpServiceAddressConfiguration CoreAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "core")).Value;

        public ProcessingServiceConfiguration ChronoConsumerConfig =>
            ProcessingServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "chronoConsumer")).Value;

        public ServiceCredentials ServiceCredentials { get; set; }

        public string RollingFileLoggerPath { get; set; } = "..\\..\\..\\..\\logs";

    }
}