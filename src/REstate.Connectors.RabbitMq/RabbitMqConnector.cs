using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Psibr.Platform.Logging;
using Psibr.Platform.Serialization;
using RabbitMQ.Client;
using REstate.Configuration;
using REstate.Services;
using Polly;

namespace REstate.Connectors.RabbitMq
{
    public class RabbitMqConnector
        : IConnector
    {
        protected ConnectionFactory ConnectionFactory { get; set; }
        protected IStringSerializer StringSerializer { get; set; }
        public IPlatformLogger Logger { get; set; }

        public RabbitMqConnector(ConnectionFactory connectionFactory, IStringSerializer stringSerializer, IPlatformLogger logger)
        {
            ConnectionFactory = connectionFactory;
            StringSerializer = stringSerializer;
            Logger = logger;
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            return ConstructAction(machineInstance, null, code);
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, string payload, ICodeWithDatabaseConfiguration code)
        {
            return (cancellationToken) =>
            {
                var actionSettings = StringSerializer.Deserialize<ActionConfiguration>(code.CodeBody);

                if (payload == null)
                    payload = actionSettings.MessageBody;

                Policy.Handle<Exception>().WaitAndRetryForever((i) =>
                    TimeSpan.FromSeconds((System.Math.Floor(15 * (System.Math.Log10(30 * i) * i)) - 7) * (i <= 5 ? 1 : (i - 4))),
                    (ex, timespan, ctx) =>
                        Logger
                            .ForContext("retryContext", ctx)
                            .Error(ex, "An error occured while attempting to send a RabbitMq message. Will retry after {time} seconds.", timespan.Seconds))
                    .Execute(() =>
                    {
                        using (var connection = ConnectionFactory.CreateConnection())
                        using (var channel = connection.CreateModel())
                        {
                            var properties = channel.CreateBasicProperties();

                            properties.ContentType = actionSettings.ContentType ?? "application/json";
                            properties.Type = actionSettings.MessageType;
                            if (actionSettings.Headers != null)
                                properties.Headers = actionSettings.Headers
                                    .ToDictionary<KeyValuePair<string, string>,
                                        string, object>(kvp => kvp.Key, kvp => kvp.Value);

                            properties.Headers = properties.Headers ?? new Dictionary<string, object>();

                            properties.Headers.Add("MachineInstanceId", machineInstance.MachineInstanceId);
                            properties.Headers.Add("MachineDefinitionId", machineInstance.MachineDefinitionId);

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
                        }
                    });                

                return Task.CompletedTask;
            };
        }

        public Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            throw new NotSupportedException();
        }
    }
}
