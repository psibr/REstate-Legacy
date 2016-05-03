using System.Threading.Tasks;
using Psibr.Platform.Logging;
using Psibr.Platform.Serialization;
using RabbitMQ.Client;
using REstate.Services;

namespace REstate.Connectors.RabbitMq
{

    public class RabbitMqConnectorFactory
        : IConnectorFactory
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IStringSerializer _stringSerializer;
        private readonly IPlatformLogger _logger;

        public RabbitMqConnectorFactory(ConnectionFactory connectionFactory,
            IStringSerializer stringSerializer, IPlatformLogger logger)
        {
            _connectionFactory = connectionFactory;
            _stringSerializer = stringSerializer;
            _logger = logger;
        }

        public Task<IConnector> BuildConnector(string apiKey)
        {
            return Task.FromResult<IConnector>(new RabbitMqConnector(_connectionFactory, _stringSerializer, _logger));
        }
        
        string IConnectorFactory.ConnectorKey => ConnectorKey;

        public static string ConnectorKey => "REstate.Connectors.RabbitMq";
    }
}
