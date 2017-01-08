using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using REstate.Engine.Services;
using REstate.Logging;

namespace REstate.Engine.Connectors.RabbitMq
{
    public class RabbitMqConnector
        : IConnector
    {
        protected ConnectionFactory ConnectionFactory { get; set; }
        protected StringSerializer StringSerializer { get; set; }
        public IPlatformLogger Logger { get; set; }

        public RabbitMqConnector(ConnectionFactory connectionFactory, StringSerializer stringSerializer, IPlatformLogger logger)
        {
            ConnectionFactory = connectionFactory;
            StringSerializer = stringSerializer;
            Logger = logger;
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, State state, IDictionary<string, string> configuration)
        {
            return ConstructAction(machineInstance, state, null, null, configuration);
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, State state, string contentType, string payload, IDictionary<string, string> configuration)
        {
            return (cancellationToken) =>
            {
                string headersString = null;
                IDictionary<string, string> headers = null;
                if(configuration != null && configuration.TryGetValue("headers", out headersString))
                {
                    headers = StringSerializer.Deserialize<IDictionary<string, string>>(headersString);
                }

                var actionSettings = new ActionConfiguration(configuration, headers);

                if (payload == null)
                    payload = actionSettings.MessageBody;


                using (var connection = ConnectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var properties = channel.CreateBasicProperties();

                    properties.ContentType = contentType ?? actionSettings.ContentType ?? "text/plain";
                    properties.Type = actionSettings.MessageType;
                    if (actionSettings.Headers != null)
                        properties.Headers = actionSettings.Headers
                            .ToDictionary<KeyValuePair<string, string>,
                                string, object>(kvp => kvp.Key, kvp => kvp.Value);

                    properties.Headers = properties.Headers ?? new Dictionary<string, object>();

                    properties.Headers.Add("MachineId", machineInstance.MachineId);

                    try
                    {
                        channel.BasicPublish(actionSettings.ExchangeName,
                            actionSettings.MessageType, properties, Encoding.UTF8.GetBytes(payload));

                        Logger
                            .ForContext("ActionConfiguration", actionSettings, true)
                            .Debug("Successfully published RabbitMq action message. " +
                                   "MessageType: {messageType}, Payload: {payload}",
                                   actionSettings.MessageType, payload);
                    }
                    catch (Exception ex)
                    {
                        ex.Data.Add("VirtualHost", ConnectionFactory.VirtualHost);
                        ex.Data.Add("HostName", ConnectionFactory.HostName);
                        ex.Data.Add("ExchangeName", actionSettings.ExchangeName);
                        ex.Data.Add("RoutingKey", properties.Type);
                        ex.Data.Add("Headers", properties.Headers);
                        ex.Data.Add("Payload", payload);

                        Logger.Error(ex, "Failed to publish action message to RabbitMq. " +
                                         "MessageType: {messageType}, Payload: {payload}",
                                         actionSettings.MessageType, payload);

                        throw;
                    }

                    return Task.CompletedTask;
                }
            };
        }

        public Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, IDictionary<string, string> configuration)
        {
            throw new NotSupportedException();
        }

        public static string ConnectorKey { get; } = "RabbitMQ";

        string IConnector.ConnectorKey => ConnectorKey;
    }
}