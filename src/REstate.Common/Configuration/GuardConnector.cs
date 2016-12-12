using System.Collections.Generic;

public class GuardConnector
    {
        public string ConnectorKey { get; set; }
        public IDictionary<string, string> Configuration { get; set; }
        public string Description { get; set; }
    }