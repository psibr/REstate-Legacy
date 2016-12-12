using System.Collections.Generic;

namespace REstate.Configuration
{
    public class EntryConnector
    {
        public string ConnectorKey { get; set; }
        public IDictionary<string, string> Configuration { get; set; }
        public string Description { get; set; }
        public ExceptionTransition FailureTransition { get; set; }
    }
}
