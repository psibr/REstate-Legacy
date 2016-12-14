using REstate.Engine.Services;
using REstate.Logging;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Threading;

namespace REstate.Engine.Connectors.RabbitMq
{
    public class RabbitMqConnectorFactory
        : IConnectorFactory
    {
        private readonly ConnectionFactory _rabbitConnectionFactory;

        private readonly StringSerializer _stringSerializer;
        private readonly IPlatformLogger _logger;

        public RabbitMqConnectorFactory(ConnectionFactory rabbitConnectionFactory, StringSerializer stringSerializer, IPlatformLogger logger)
        {
            _rabbitConnectionFactory = rabbitConnectionFactory;

            _stringSerializer = stringSerializer;
            _logger = logger;
        }

        public string ConnectorKey => RabbitMqConnector.ConnectorKey;
        public bool IsActionConnector { get; } = true;
        public bool IsGuardConnector { get; } = false;
        public string ConnectorSchema { get; set; } = "{ }";

        public Task<IConnector> BuildConnectorAsync(string apiKey, CancellationToken cancellationToken)
        {
            var connector = new RabbitMqConnector(_rabbitConnectionFactory, _stringSerializer, _logger);

            return Task.FromResult<IConnector>(connector);
        }
    }
}
