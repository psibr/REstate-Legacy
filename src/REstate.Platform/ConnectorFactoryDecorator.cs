using System;
using System.Threading.Tasks;
using REstate.Services;

namespace REstate.Platform
{
    public class ConnectorFactoryDecorator
        : IConnectorFactory
    {
        private readonly IConnectorFactory _factory;
        private readonly IConnectorDecorator _decorator;

        public ConnectorFactoryDecorator(IConnectorFactory factory, IConnectorDecorator decorator)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _factory = factory;
            _decorator = decorator;
        }

        public async Task<IConnector> BuildConnector(string apiKey)
        {
            var connector = await _factory.BuildConnector(apiKey);

            return _decorator.Decorate(connector);
        }

        public string ConnectorKey => _factory.ConnectorKey;

        public bool IsActionConnector { get; } = true;
        public bool IsGuardConnector { get; } = false;
        public string ConnectorSchema { get; set; } = null;
    }
}