using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Engine.Services
{
    public class DefaultConnectorFactoryResolver
        : IConnectorFactoryResolver
    {
        private readonly IDictionary<string, IConnectorFactory> _connectorFactories;

        public DefaultConnectorFactoryResolver()
        {
            _connectorFactories = new Dictionary<string, IConnectorFactory>();
        }

        public DefaultConnectorFactoryResolver(IEnumerable<IConnectorFactory> connectorFactories)
        {
            _connectorFactories = connectorFactories
                .ToDictionary(kvp => kvp.ConnectorKey, kvp => kvp);
        }

        public virtual IConnectorFactory ResolveConnectorFactory(string connectorKey)
        {
            IConnectorFactory connectorFactory;
            if (!_connectorFactories.TryGetValue(connectorKey, out connectorFactory))
                throw new ArgumentException($"No connector factory exists matching connectorKey: \"{connectorKey}\".", nameof(connectorKey));

            return connectorFactory;
        }
    }
}
