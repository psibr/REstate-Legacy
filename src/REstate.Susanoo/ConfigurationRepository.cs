using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using REstate.Configuration;
using REstate.Repositories;
using Susanoo;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace REstate.Susanoo
{
    public class ConfigurationRepository
        : REstateContextualRepository, IConfigurationRepository
    {

        public ConfigurationRepository(Repository root)
            : base(root)
        {
            Code = new CodeConfigurationRepository(root);
        }

        public ICodeConfigurationRepository Code { get; }

        public async Task<IStateMachineConfiguration> RetrieveMachineConfiguration(int machineDefinitionId,
            CancellationToken cancellationToken)
        {
            var results = (await CommandManager.Instance
                .DefineCommand("[dbo].[LoadMachineDefinition]", CommandType.StoredProcedure)
                .DefineResults(
                    typeof(MachineDefinition),
                    typeof(Configuration.State),
                    typeof(Configuration.Trigger),
                    typeof(Transition),
                    typeof(IgnoreRule),
                    typeof(Guard),
                    typeof(StateAction),
                    typeof(CodeWithDatabaseConfiguration))
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { machineDefinitionId }, cancellationToken))
                .ToArray();

            var machineDefinition = results[0].Cast<IMachineDefinition>().SingleOrDefault();
            var states = results[1].Cast<IState>();
            var triggers = results[2].Cast<ITrigger>().ToList();
            var transitionResults = results[3].Cast<ITransition>();
            var ignoreRuleResults = results[4].Cast<IIgnoreRule>();
            var guards = results[5].Cast<IGuard>().ToList();
            var stateActions = results[6].Cast<IStateAction>().ToList();
            var codeElements = results[7].Cast<ICodeWithDatabaseConfiguration>().ToList();


            if (machineDefinition == null)
                return null;

            ICollection<IStateConfiguration> stateConfigurations = states
                .GroupJoin(stateActions,
                    state => state.StateName, action => action.StateName,
                    (state, actions) => new StateConfiguration
                    {
                        State = state,
                        OnEntryAction = actions.SingleOrDefault(action => action.PurposeName == "OnEntry"),
                        OnEntryFromAction = actions.SingleOrDefault(action => action.PurposeName == "OnEntryFrom"),
                        OnExitAction = actions.SingleOrDefault(action => action.PurposeName == "OnExit")
                    })
                .GroupJoin(transitionResults,
                    stateConfiguration => stateConfiguration.State.StateName, t => t.StateName,
                    (stateConfiguration, transitions) => new StateConfiguration
                    {
                        State = stateConfiguration.State,
                        OnEntryAction = stateConfiguration.OnEntryAction,
                        OnEntryFromAction = stateConfiguration.OnEntryFromAction,
                        OnExitAction = stateConfiguration.OnExitAction,
                        Transitions = transitions.ToList()
                    })
                .GroupJoin(ignoreRuleResults,
                    stateConfiguration => stateConfiguration.State.StateName, i => i.StateName,
                    (stateConfiguration, ignoreRules) => new StateConfiguration
                    {
                        State = stateConfiguration.State,
                        OnEntryAction = stateConfiguration.OnEntryAction,
                        OnEntryFromAction = stateConfiguration.OnEntryFromAction,
                        OnExitAction = stateConfiguration.OnExitAction,
                        Transitions = stateConfiguration.Transitions,
                        IgnoreRules = ignoreRules.ToList()
                    })
                .Cast<IStateConfiguration>()
                .ToList();

            var machineConfiguration = new StateMachineConfiguration
            {
                MachineDefinition = machineDefinition,
                StateConfigurations = stateConfigurations,
                Triggers = triggers,
                Guards = guards,
                CodeElements = codeElements
            };

            return machineConfiguration;
        }

        public async Task<IStateMachineConfiguration> RetrieveMachineConfiguration(Guid machineInstanceGuid,
            CancellationToken cancellationToken)
        {
            IStateMachineConfiguration configuration;
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                var machineDefinitionId = await CommandManager.Instance
                    .DefineCommand(
                        "SELECT TOP 1 MachineDefinitionId FROM MachineInstances WHERE MachineInstanceId = @MachineInstanceGuid",
                        CommandType.Text)
                    .Realize()
                    .ExecuteScalarAsync<int?>(DatabaseManagerPool.DatabaseManager, new { MachineInstanceGuid = machineInstanceGuid },
                        cancellationToken);

                if (machineDefinitionId == null || machineDefinitionId.Value <= 0)
                    throw new ArgumentException("Not a valid instance identifier.", nameof(machineDefinitionId));

                configuration = await RetrieveMachineConfiguration(machineDefinitionId.Value, cancellationToken);

                scope.Complete();
            }

            return configuration;
        }

        public async Task<IMachineDefinition> DefineMachine(IMachineDefinition machineDefinition,
            CancellationToken cancellationToken)
        {
            return (await CommandManager.Instance
                .DefineCommand<IMachineDefinition>(
                    "INSERT INTO MachineDefinitions " +
                    "VALUES(@MachineName, @MachineDescription, NULL, @AutoIgnoreNotConfiguredTriggers, 0);\n\n" +
                    "SELECT * FROM MachineDefinitions WHERE MachineDefinitionId = @@IDENTITY",
                    CommandType.Text)
                .ExcludeProperty(o => o.MachineDefinitionId)
                .ExcludeProperty(o => o.InitialStateName)
                .ExcludeProperty(o => o.IsActive)
                .DefineResults<MachineDefinition>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, machineDefinition, cancellationToken))
                .Cast<IMachineDefinition>()
                .SingleOrDefault();
        }

        public async Task<ICollection<IState>> DefineStates(ICollection<IState> states,
            CancellationToken cancellationToken)
        {
            var command = CommandManager.Instance
                .DefineCommand<IState>(
                    "INSERT INTO States VALUES(@MachineDefinitionId, @StateName, @ParentStateName, @StateDescription);\n\n" +
                    "SELECT * FROM States WHERE MachineDefinitionId = @MachineDefinitionId AND StateName = @StateName;",
                    CommandType.Text)
                .SendNullValues()
                .DefineResults<Configuration.State>()
                .Realize();

            var newStates = Enumerable.Empty<IState>();

            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                // Cannot await in a linq expression.
                foreach (var state in states.OrderBy(o => o.ParentStateName))
                {
                    newStates = newStates.Union(
                        await command.ExecuteAsync(DatabaseManagerPool.DatabaseManager,
                            state, cancellationToken));
                }

                scope.Complete();
            }

            return newStates.ToList();
        }

        public async Task<ICollection<ITrigger>> DefineTriggers(ICollection<ITrigger> triggers,
            CancellationToken cancellationToken)
        {
            var command = CommandManager.Instance
                .DefineCommand<ITrigger>(
                    "INSERT INTO Triggers VALUES(@MachineDefinitionId, @TriggerName, @TriggerDescription, @IsActive);\n\n" +
                    "SELECT * FROM Triggers WHERE MachineDefinitionId = @MachineDefinitionId AND TriggerName = @TriggerName;",
                    CommandType.Text)
                .DefineResults<Configuration.Trigger>()
                .Realize();

            var newTriggers = Enumerable.Empty<ITrigger>();

            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                // Cannot await in a linq expression.
                foreach (var trigger in triggers)
                {
                    newTriggers = newTriggers.Union(
                        await command.ExecuteAsync(DatabaseManagerPool.DatabaseManager,
                            trigger, cancellationToken));
                }

                scope.Complete();
            }

            return newTriggers.ToList();

        }

        public async Task<ICollection<ITransition>> DefineTransitions(ICollection<ITransition> transitions,
            CancellationToken cancellationToken)
        {
            var command = CommandManager.Instance
                .DefineCommand<ITransition>(
                    "INSERT INTO Transitions VALUES(@MachineDefinitionId, @StateName, @TriggerName, @ResultantStateName, @GuardId, @IsActive);\n\n" +
                    "SELECT * FROM Transitions WHERE MachineDefinitionId = @MachineDefinitionId AND StateName = @StateName AND TriggerName = @TriggerName;",
                    CommandType.Text)
                .SendNullValues()
                .DefineResults<Transition>()
                .Realize();

            var newTransitions = Enumerable.Empty<ITransition>();

            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                // Cannot await in a linq expression.
                foreach (var transition in transitions)
                {
                    newTransitions = newTransitions.Union(
                        await command.ExecuteAsync(DatabaseManagerPool.DatabaseManager,
                            transition, cancellationToken));
                }

                scope.Complete();
            }

            return newTransitions.ToList();
        }

        public async Task<ICollection<IIgnoreRule>> DefineIgnoreRules(ICollection<IIgnoreRule> ignoreRules,
            CancellationToken cancellationToken)
        {
            var command = CommandManager.Instance
                .DefineCommand<IIgnoreRule>(
                    "INSERT INTO IgnoreRules VALUES(@MachineDefinitionId, @StateName, @TriggerName, @IsActive);\n\n" +
                    "SELECT * FROM IgnoreRules WHERE MachineDefinitionId = @MachineDefinitionId AND StateName = @StateName AND TriggerName = @TriggerName;",
                    CommandType.Text)
                .SendNullValues()
                .DefineResults<IgnoreRule>()
                .Realize();

            var newIgnoreRules = Enumerable.Empty<IIgnoreRule>();

            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                // Cannot await in a linq expression.
                foreach (var ignoreRule in ignoreRules)
                {
                    newIgnoreRules = newIgnoreRules.Union(
                        await command.ExecuteAsync(DatabaseManagerPool.DatabaseManager,
                            ignoreRule, cancellationToken));
                }

                scope.Complete();
            }

            return newIgnoreRules.ToList();
        }

        public async Task<IMachineDefinition> SetInitialState(int machineDefinitionId,
            string initialStateName, CancellationToken cancellationToken)
        {
            return (await CommandManager.Instance
                .DefineCommand(
                    "UPDATE MachineDefinitions SET InitialStateName = @InitialStateName WHERE MachineDefinitionId = @MachineDefinitionId;\n\n" +
                    "SELECT * FROM MachineDefinitions WHERE MachineDefinitionId = @MachineDefinitionId;",
                    CommandType.Text)
                .DefineResults<MachineDefinition>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    InitialStateName = initialStateName, MachineDefinitionId = machineDefinitionId
                },
                    cancellationToken))
                .Single();
        }

        public async Task<ICollection<IGuard>> DefineGuards(ICollection<IGuard> guards,
            CancellationToken cancellationToken)
        {
            var command = CommandManager.Instance
                .DefineCommand<IGuard>(
                    "INSERT INTO Guards VALUES(@GuardName, @GuardDescription, @CodeElementId);\n\n" +
                    "SELECT * FROM Guards WHERE GuardId = @@IDENTITY;",
                    CommandType.Text)
                .ExcludeProperty(o => o.GuardId)
                .SendNullValues()
                .DefineResults<Guard>()
                .Realize();

            var newGuards = Enumerable.Empty<IGuard>();

            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                // Cannot await in a linq expression.
                foreach (var guard in guards)
                {
                    newGuards = newGuards.Union(
                        await command.ExecuteAsync(DatabaseManagerPool.DatabaseManager,
                            guard, cancellationToken));
                }

                scope.Complete();
            }

            return newGuards.ToList();
        }

        public async Task<ITransition> UpdateTransition(ITransition transition, CancellationToken cancellationToken)
        {
            if (transition == null) throw new ArgumentNullException(nameof(transition));
            if (transition.MachineDefinitionId <= 0)
                throw new ArgumentException("MachineDefinitionId is a required property.", nameof(transition));
            if (string.IsNullOrWhiteSpace(transition.StateName))
                throw new ArgumentException("StateName is a required property.", nameof(transition));
            if (string.IsNullOrWhiteSpace(transition.TriggerName))
                throw new ArgumentException("TriggerName is a required property.", nameof(transition));

            return (await CommandManager.Instance
                .DefineCommand<ITransition>(
                    "UPDATE Transitions SET ResultantStateName = @ResultantStateName, GuardId = @GuardId, IsActive = @IsActive\n" +
                    "WHERE MachineDefinitionId = @MachineDefinitionId AND StateName = @StateName AND TriggerName = @TriggerName;\n\n" +
                    "SELECT * FROM Transitions WHERE MachineDefinitionId = @MachineDefinitionId AND StateName = @StateName AND TriggerName = @TriggerName;",
                    CommandType.Text)
                .SendNullValues()
                .DefineResults<Transition>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, transition, cancellationToken))
                .Single();
        }

        public async Task<IGuard> UpdateGuard(IGuard guard, CancellationToken cancellationToken)
        {
            if (guard == null) throw new ArgumentNullException(nameof(guard));
            if (guard.GuardId <= 0) throw new ArgumentException("GuardId is a required property.", nameof(guard));
            if (string.IsNullOrWhiteSpace(guard.GuardName))
                throw new ArgumentException("GuardName is a required property.", nameof(guard));

            return (await CommandManager.Instance
                .DefineCommand<IGuard>(
                    "UPDATE Guards SET GuardName = @GuardName, GuardDescription = @GuardDescription, CodeElementId = @CodeElementId\n" +
                    "WHERE GuardId = @GuardId;\n\n" +
                    "SELECT * FROM Guards WHERE GuardId = @GuardId;", CommandType.Text)
                .SendNullValues()
                .DefineResults<Guard>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, guard, cancellationToken))
                .Single();
        }

        public async Task<IMachineDefinition> ToggleMachineDefinitionActive(int machineDefinitionId, bool isActive,
            CancellationToken cancellationToken)
        {
            if (machineDefinitionId <= 0)
                throw new ArgumentException("machineDefinitionId is required.", nameof(machineDefinitionId));

            return (await CommandManager.Instance
                .DefineCommand("UPDATE MachineDefinitions SET IsActive = @IsActive\n" +
                               "WHERE MachineDefinitionId = @MachineDefinitionId;\n\n" +
                               "SELECT* FROM MachineDefinitions WHERE MachineDefinitionId = @MachineDefinitionId; ",
                    CommandType.Text)
                .DefineResults<MachineDefinition>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { MachineDefinitionId = machineDefinitionId, IsActive = isActive }, cancellationToken))
                .Single();
        }

        public async Task<IMachineDefinition> UpdateMachineDefinition(IMachineDefinition machineDefinition,
            CancellationToken cancellationToken)
        {
            if (machineDefinition == null) throw new ArgumentNullException(nameof(machineDefinition));
            if (machineDefinition.MachineDefinitionId <= 0)
                throw new ArgumentException("MachineDefinition is a required property.", nameof(machineDefinition));
            if (string.IsNullOrWhiteSpace(machineDefinition.MachineName))
                throw new ArgumentException("MachineName is a required property.", nameof(machineDefinition));
            if (string.IsNullOrWhiteSpace(machineDefinition.InitialStateName))
                throw new ArgumentException("InitialStateName is a required property.", nameof(machineDefinition));

            return (await CommandManager.Instance
                .DefineCommand<IMachineDefinition>(
                    "UPDATE MachineDefinitions SET MachineName = @MachineName, MachineDescription = @MachineDescription,\n" +
                    "InitialStateName = @InitialStateName, AutoIgnoreNotConfiguredTriggers = @AutoIgnoreNotConfiguredTriggers,\n" +
                    "IsActive = @IsActive\n" +
                    "WHERE MachineDefinitionId = @MachineDefinitionId;\n\n" +
                    "SELECT * FROM MachineDefinitions WHERE MachineDefinitionId = @MachineDefinitionId; ",
                    CommandType.Text)
                .SendNullValues()
                .DefineResults<MachineDefinition>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, machineDefinition, cancellationToken))
                .Single();
        }
    }
}
