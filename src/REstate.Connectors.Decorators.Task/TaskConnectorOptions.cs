using System;
using Psibr.Platform.Logging;
using REstate.Configuration;

namespace REstate.Connectors.Decorators.Task
{
    public class TaskConnectorOptions
    {
        public TaskConnectorOptions()
        {
            FailureAction = (task, machineInstance, code, logger, payload) =>
                logger.ForContext("machineInstance", machineInstance, true)
                    .ForContext("code", code, true)
                    .Error(task.Exception,
                        "Task encountered an exception and may not have finished executing.");

            SuccessAction = (task, machineInstance, code, logger, payload) =>
                logger.Debug("Task completion successful for connector action.");
        }

        public Action<System.Threading.Tasks.Task, IStateMachine, Code, IPlatformLogger, string> SuccessAction { get; set; }

        public Action<System.Threading.Tasks.Task, IStateMachine, Code, IPlatformLogger, string> FailureAction { get; set; }
    }
}