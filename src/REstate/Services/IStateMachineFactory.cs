using REstate.Configuration;
using REstate.Repositories.Instances;
using System;

namespace REstate.Services
{
    public interface IStateMachineFactory
    {
        IStateMachine ConstructFromConfiguration(string apiKey, string machineInstanceId,
            IStateMachineConfiguration configuration,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory);

        IStateMachine ConstructFromConfiguration(string apiKey, IStateMachineConfiguration configuration);
    }
}