using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Psibr.Platform.Logging;
using REstate.Configuration;
using REstate.Services;

namespace REstate.Platform
{
    public class TaskConnectorDecorator
        : IConnector
    {
        private readonly IConnector _connector;

        public TaskConnectorDecorator(IConnector connector, IPlatformLogger logger)
        {
            _connector = connector;
            Logger = logger.ForContext<TaskConnectorDecorator>()
                .ForContext("connectorType", connector.GetType().AssemblyQualifiedName);
        }

        protected IPlatformLogger Logger { get; }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code)
        {
            return (cancellationToken) =>
            {

                return Task.Run(() => _connector.ConstructAction(machineInstance, code)(cancellationToken))
                    .ContinueWith(task =>
                    {
                        if (task.Exception == null)
                            Logger.Debug("Task completion successful for connector action.");
                        else
                            Logger.ForContext("machineInstance", machineInstance, true)
                                .ForContext("code", code, true)
                                .Error(task.Exception, "Task encountered an exception and may not have finished executing.");
                    });

            };
        }

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, string payload, ICodeWithDatabaseConfiguration code)
        {
            return (cancellationToken) =>
            {

                return Task.Run(() => _connector.ConstructAction(machineInstance, payload, code)(cancellationToken))
                    .ContinueWith(task =>
                    {
                        if (task.Exception == null)
                            Logger.Debug("Task completion successful for connector action.");
                        else
                            Logger.ForContext("machineInstance", machineInstance, true)
                                .ForContext("code", code, true)
                                .Error(task.Exception, "Task encountered an exception and may not have finished executing.");
                    });

            };
        }

        public Func<CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, ICodeWithDatabaseConfiguration code) =>
            _connector.ConstructPredicate(machineInstance, code);
    }
}
