using System.Collections.Generic;

namespace REstate.Configuration
{
    public class Code
    {
        public string Name { get; set; }
        public string ConnectorKey { get; set; }
        public IDictionary<string, string> Configuration { get; set; }
        public string Description { get; set; }
    }
}
