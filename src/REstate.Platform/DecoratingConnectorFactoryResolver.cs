using System;
using System.Collections.Generic;
using System.Linq;
using REstate.Services;

namespace REstate.Platform
{
    public class DecoratingConnectorFactoryResolver
        : DefaultConnectorFactoryResolver
    {
        private readonly IDictionary<string, IEnumerable<IConnectorDecorator>> _decorators;

        public DecoratingConnectorFactoryResolver(IEnumerable<IConnectorFactory> connectorFactories,
            ConnectorDecoratorAssociations decorators)
            : base(connectorFactories)
        {
            if (decorators == null) throw new ArgumentNullException(nameof(decorators));

            _decorators = decorators;
        }

        public DecoratingConnectorFactoryResolver(IEnumerable<IConnectorFactory> connectorFactories,
            IDictionary<string, IEnumerable<IConnectorDecorator>> decorators)
            : base(connectorFactories)
        {
            _decorators = decorators;
        }

        public DecoratingConnectorFactoryResolver(IEnumerable<IConnectorFactory> connectorFactories)
            : this(connectorFactories, new IConnectorDecorator[0])
        {
        }

        public DecoratingConnectorFactoryResolver(IEnumerable<IConnectorFactory> connectorFactories,
            params IConnectorDecorator[] decorators)
            : base(connectorFactories)
        {
            _decorators = new Dictionary<string, IEnumerable<IConnectorDecorator>>();

            foreach (var connectorFactory in connectorFactories)
            {
                _decorators.Add(connectorFactory.ConnectorKey, decorators);
            }
        }

        public override IConnectorFactory ResolveConnectorFactory(string connectorKey)
        {
            var connectorFactory = base.ResolveConnectorFactory(connectorKey);

            IEnumerable<IConnectorDecorator> decorators;
            if (_decorators.TryGetValue(connectorKey, out decorators))
            {
                connectorFactory = decorators
                    .Aggregate(connectorFactory, (current, connectorDecorator) =>
                        new ConnectorFactoryDecorator(current, connectorDecorator));
            }

            return connectorFactory;
        }
    }
}