using System.Collections.Generic;

namespace REstate.Configuration
{
    public class ServiceEntryConnector
    {
        public string ConnectorKey { get; set; }
        public IDictionary<string, string> Configuration { get; set; }
        public string Description { get; set; }
    }
}
