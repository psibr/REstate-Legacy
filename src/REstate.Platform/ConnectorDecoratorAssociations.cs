using System.Collections.Generic;

namespace REstate.Platform
{
    public class ConnectorDecoratorAssociations
    {
        public IDictionary<string, IEnumerable<IConnectorDecorator>> Associations { get; set; } =
            new Dictionary<string, IEnumerable<IConnectorDecorator>>(0);
    }
}