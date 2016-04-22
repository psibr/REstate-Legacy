using Psibr.Platform.Logging;
using REstate.Platform;
using REstate.Services;

namespace REstate.Connectors.Decorators.Task
{
    public class TaskConnectorDecorator
        : IConnectorDecorator
    {
        private readonly IPlatformLogger _logger;
        private readonly TaskConnectorOptions _options;

        public TaskConnectorDecorator(IPlatformLogger logger, TaskConnectorOptions options = null)
        {
            _logger = logger;
            _options = options ?? new TaskConnectorOptions();
        }

        public IConnector Decorate(IConnector connector)
        {
            return new TaskConnector(connector, _logger, _options);
        }
    }
}
