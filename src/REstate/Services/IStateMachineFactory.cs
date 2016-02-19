using System;
using REstate.Configuration;

namespace REstate.Services
{
    public interface IStateMachineFactory
    {
        IStateMachine ConstructFromConfiguration(string apiKey, Guid machineInstanceGuid, IStateMachineConfiguration configuration);

        IStateMachine ConstructFromConfiguration(string apiKey, IStateMachineConfiguration configuration);
    }
}