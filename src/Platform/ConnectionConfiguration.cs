using System.Collections.Generic;

namespace Platform
{
    public class ConnectionConfiguration
    {
        public string ConnectionString { get; set; }

        public string ProviderName { get; set; }

        public IDictionary<string, string> AdditionalOptions { get; set; }

        public string[] Tags { get; set; }
    }
}