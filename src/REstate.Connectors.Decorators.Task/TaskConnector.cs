using System;
using System.Threading;
using Psibr.Platform.Logging;
using REstate.Configuration;
using REstate.Services;

namespace REstate.Connectors.Decorators.Task
{
    using System.Threading.Tasks;

    public class TaskConnector
        : IConnector
    {
        protected TaskConnectorOptions Options { get; }
        private readonly IConnector _connector;

        public TaskConnector(IConnector connector, IPlatformLogger logger, TaskConnectorOptions options = null)
        {
            Options = options ?? new TaskConnectorOptions();
            _connector = connector;
            Logger = logger.ForContext<TaskConnectorDecorator>()
                .ForContext("connectorType", connector.GetType().AssemblyQualifiedName);
        }

        protected IPlatformLogger Logger { get; }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, Code code)
        {
            return cancellationToken =>
            {

                return Task.Run(() => _connector.ConstructAction(machineInstance, code)(cancellationToken))
                    .ContinueWith(task =>
                    {
                        if (task.Exception == null)
                            Options.SuccessAction(task, machineInstance, code, Logger, null);
                        else
                            Options.FailureAction(task, machineInstance, code, Logger, null);
                    });

            };
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, string payload, Code code)
        {
            return cancellationToken =>
            {

                return Task.Run(() => _connector.ConstructAction(machineInstance, payload, code)(cancellationToken))
                    .ContinueWith(task =>
                    {
                        if (task.Exception == null)
                            Options.SuccessAction(task, machineInstance, code, Logger, payload);
                        else
                            Options.FailureAction(task, machineInstance, code, Logger, payload);
                    });

            };
        }

        public Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, Code code) =>
            _connector.ConstructPredicate(machineInstance, code);

        public string ConnectorKey => $"Task<{_connector.ConnectorKey}>";
    }
}