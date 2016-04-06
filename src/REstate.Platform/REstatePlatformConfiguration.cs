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

        public HttpServiceAddressConfiguration InstancesAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "instances")).Value;

        public HttpServiceAddressConfiguration ConfigurationAddress =>
            HttpServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "configuration")).Value;

        public ProcessingServiceConfiguration ChronoConsumerConfig =>
            ProcessingServices.SingleOrDefault(kvp => string.Equals(kvp.Key, "chronoConsumer")).Value;
    }
}