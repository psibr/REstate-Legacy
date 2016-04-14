using System.Collections.Generic;

namespace REstate.Connectors.RabbitMq
{
    public class ActionConfiguration
    {
        public string ExchangeName { get; set; }

        public string MessageType { get; set; }

        public string ContentType { get; set; }

        public string MessageBody { get; set; }

        public IDictionary<string, string> Headers { get; set; }
    }
}