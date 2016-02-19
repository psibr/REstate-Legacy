using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Repositories
{
    public interface IConfigurationRepository
    {
        ICodeConfigurationRepository Code { get; }

        Task<IStateMachineConfiguration> RetrieveMachineConfiguration(int machineDefinitionId,
            CancellationToken cancellationToken);

        Task<IStateMachineConfiguration> RetrieveMachineConfiguration(Guid machineInstanceGuid, CancellationToken cancellationToken);

        Task<IMachineDefinition> DefineMachine(IMachineDefinition machineDefinition, CancellationToken cancellationToken);

        Task<ICollection<IState>> DefineStates(ICollection<IState> states, CancellationToken cancellationToken);

        Task<ICollection<ITrigger>> DefineTriggers(ICollection<ITrigger> triggers, CancellationToken cancellationToken);

        Task<ICollection<ITransition>> DefineTransitions(ICollection<ITransition> triggers, CancellationToken cancellationToken);

        Task<ICollection<IIgnoreRule>> DefineIgnoreRules(ICollection<IIgnoreRule> ignoreRules, CancellationToken cancellationToken);

        Task<IMachineDefinition> SetInitialState(int machineDefinitionId,
            string initialStateName, CancellationToken cancellationToken);

        Task<ICollection<IGuard>> DefineGuards(ICollection<IGuard> states, CancellationToken cancellationToken);

        Task<ITransition> UpdateTransition(ITransition transition, CancellationToken cancellationToken);

        Task<IGuard> UpdateGuard(IGuard guard, CancellationToken cancellationToken);

        Task<IMachineDefinition> ToggleMachineDefinitionActive(int machineDefinitionId, bool isActive,
            CancellationToken cancellationToken);

        Task<IMachineDefinition> UpdateMachineDefinition(IMachineDefinition machineDefinition,
            CancellationToken cancellationToken);
    }
}