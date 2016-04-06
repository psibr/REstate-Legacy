using REstate.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Repositories.Configuration
{
    public interface IMachineConfigurationRepository
    {
        Task<IStateMachineConfiguration> RetrieveMachineConfiguration(string machineDefinitionId, bool loadCode,
            CancellationToken cancellationToken);

        Task<IStateMachineConfiguration> RetrieveMachineConfigurationForInstance(string machineInstanceId, bool loadCode, CancellationToken cancellationToken);

        Task<IStateMachineConfiguration> DefineStateMachine(IStateMachineConfiguration stateMachineConfiguration, string previousVersionId, CancellationToken cancellationToken);

        Task<IStateMachineConfiguration> DefineStateMachine(IStateMachineConfiguration stateMachineConfiguration, CancellationToken cancellationToken);
    }
}