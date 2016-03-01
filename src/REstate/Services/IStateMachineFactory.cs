using System;
using REstate.Configuration;
using REstate.Repositories.Instances;

namespace REstate.Services
{
    public interface IStateMachineFactory
    {
        IStateMachine ConstructFromConfiguration(string apiKey, Guid machineInstanceGuid,
            IStateMachineConfiguration configuration,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory);

        IStateMachine ConstructFromConfiguration(string apiKey, IStateMachineConfiguration configuration);
    }
}