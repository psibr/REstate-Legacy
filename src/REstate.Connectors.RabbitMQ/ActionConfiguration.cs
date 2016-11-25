using System;
using System.Collections.Generic;

namespace REstate.Engine.Connectors.RabbitMq
{
    public class ActionConfiguration
    {
        public ActionConfiguration() { }

        internal ActionConfiguration(IDictionary<string, string> configuration, IDictionary<string, string> headers)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            string exchangeName;
            string messageType;
            string contentType;
            string messageBody;

            if (configuration.TryGetValue(nameof(exchangeName), out exchangeName))
                ExchangeName = exchangeName;
            else
                throw new ArgumentException($"Configuration did not contain {nameof(exchangeName)}.", nameof(configuration));

            if (configuration.TryGetValue(nameof(messageType), out messageType))
                MessageType = messageType;
            else
                throw new ArgumentException("Configuration did not contain {nameof(messageType)}.", nameof(configuration));

            if (configuration.TryGetValue(nameof(contentType), out contentType))
                ContentType = contentType;
            else
                throw new ArgumentException("Configuration did not contain {nameof(contentType)}.", nameof(configuration));

            if (configuration.TryGetValue(nameof(messageBody), out messageBody))
                MessageBody = messageBody;
            else
                throw new ArgumentException("Configuration did not contain {nameof(messageBody)}.", nameof(configuration));

            Headers = headers ?? new Dictionary<string, string>();
        }

        public string ExchangeName { get; set; }

        public string MessageType { get; set; }

        public string ContentType { get; set; }

        public string MessageBody { get; set; }

        public IDictionary<string, string> Headers { get; set; }
    }
}