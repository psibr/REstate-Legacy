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
    public class MachineConfigurationRepository
        : ConfigurationContextualRepository, IMachineConfigurationRepository
    {

        public MachineConfigurationRepository(ConfigurationRepository root)
            : base(root)
        {
        }


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
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
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
                .SendNullValues()
                .DefineResults<MachineDefinition>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, machineDefinition, cancellationToken))
                .Cast<IMachineDefinition>()
                .SingleOrDefault();
        }

        public async Task<ICollection<IState>> DefineStates(ICollection<IState> states,
            CancellationToken cancellationToken)
        {
            var newStates = (await CommandManager.Instance
                .DefineCommand("dbo.DefineStates", CommandType.StoredProcedure)
                .IncludePropertyAsStructured("States", "StateTable")
                .DefineResults<Configuration.State>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager,
                    new { States = states }, cancellationToken))
                .Cast<IState>()
                .ToList();

            return newStates;

        }

        public async Task<ICollection<ITrigger>> DefineTriggers(ICollection<ITrigger> triggers,
            CancellationToken cancellationToken)
        {
            var newTriggers = (await CommandManager.Instance
                .DefineCommand("dbo.DefineTriggers", CommandType.StoredProcedure)
                .IncludePropertyAsStructured("Triggers", "TriggerTable")
                .DefineResults<Configuration.Trigger>()
                .Realize()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager,
                    new { Triggers = triggers }, cancellationToken))
                .Cast<ITrigger>()
                .ToList();

            return newTriggers;
        }

        public async Task<ICollection<ITransition>> DefineTransitions(ICollection<ITransition> transitions,
            CancellationToken cancellationToken)
        {
                var newTransitions = (await CommandManager.Instance
                    .DefineCommand("dbo.DefineTransitions", CommandType.StoredProcedure)
                    .IncludePropertyAsStructured("Transitions", "TransitionTable")
                    .SendNullValues()
                    .DefineResults<Transition>()
                    .Realize()
                    .ExecuteAsync(DatabaseManagerPool.DatabaseManager,
                        new { Transitions = transitions }, cancellationToken))
                    .Cast<ITransition>()
                    .ToList();

                return newTransitions;
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
                    InitialStateName = initialStateName,
                    MachineDefinitionId = machineDefinitionId
                },
                    cancellationToken))
                .Single();
        }

        public async Task<ICollection<IGuard>> DefineGuards(ICollection<IGuard> guards,
            CancellationToken cancellationToken)
        {
            var command = CommandManager.Instance
                .DefineCommand<IGuard>(
                    "INSERT INTO Guards VALUES(@MachineDefinitionId, @GuardName, @GuardDescription, @CodeElementId);\n\n" +
                    "SELECT * FROM Guards WHERE MachineDefinitionId = @MachineDefinitionId AND GuardName = @GuardName;",
                    CommandType.Text)
                .SendNullValues()
                .DefineResults<Guard>()
                .Realize();

            var newGuards = Enumerable.Empty<IGuard>();

            using (var scope = new TransactionScope(TransactionScopeOption.Required,
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
            if (guard.MachineDefinitionId <= 0) throw new ArgumentException("MachineDefinitionId is a required property.", nameof(guard));
            if (string.IsNullOrWhiteSpace(guard.GuardName))
                throw new ArgumentException("GuardName is a required property.", nameof(guard));

            return (await CommandManager.Instance
                .DefineCommand<IGuard>(
                    "UPDATE Guards SET GuardDescription = @GuardDescription, CodeElementId = @CodeElementId\n" +
                    "WHERE MachineDefinitionId = @MachineDefinitionId AND GuardName = @GuardName;\n\n" +
                    "SELECT * FROM Guards WHERE MachineDefinitionId = @MachineDefinitionId AND GuardName = @GuardName;", CommandType.Text)
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
                               "SELECT * FROM MachineDefinitions WHERE MachineDefinitionId = @MachineDefinitionId; ",
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

        public async Task<IStateMachineConfiguration> DefineStateMachine(IStateMachineConfiguration stateMachineConfiguration, int previousVersionId,
            CancellationToken cancellationToken)
        {
            int newId;
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                var newDefinition = await DefineMachine(stateMachineConfiguration.MachineDefinition, cancellationToken);

                newId = newDefinition.MachineDefinitionId;

                var states = stateMachineConfiguration.StateConfigurations
                    .Select(sc => sc.State).ToList();

                var triggers = stateMachineConfiguration.Triggers;

                var ignoreRules = stateMachineConfiguration.StateConfigurations
                    .SelectMany(sc => sc.IgnoreRules).ToList();

                var guards = stateMachineConfiguration.Guards;

                var transitions = stateMachineConfiguration.StateConfigurations
                    .SelectMany(sc => sc.Transitions).ToList();

                var stateActions = stateMachineConfiguration.StateConfigurations
                    .SelectMany(sc => new[] { sc.OnEntryAction, sc.OnEntryFromAction, sc.OnExitAction })
                    .Where(sa => sa != null)
                    .ToList();

                states.Cast<IMachineDefinitionDependent>()
                    .Union(triggers)
                    .Union(ignoreRules)
                    .Union(guards)
                    .Union(transitions)
                    .Union(stateActions)
                    .AsParallel()
                    .ToList()
                    .ForEach(e => e.MachineDefinitionId = newId);

                //TODO: Here we should indicate this as the new version of the machine definition.

                Task.WaitAll(
                    DefineStates(states, cancellationToken),
                    DefineTriggers(triggers, cancellationToken));

                Task.WaitAll(
                    DefineIgnoreRules(ignoreRules, cancellationToken),
                    DefineGuards(guards, cancellationToken));

                Task.WaitAll(
                    DefineTransitions(transitions, cancellationToken),
                    DefineStateActions(stateActions, cancellationToken),
                    SetInitialState(newId, stateMachineConfiguration.MachineDefinition.InitialStateName, cancellationToken));

                await ToggleMachineDefinitionActive(newId, true, cancellationToken);

                transaction.Complete();
            }

            var newConfiguration = await RetrieveMachineConfiguration(newId, cancellationToken);

            return newConfiguration;
        }

        public async Task<IStateMachineConfiguration> DefineStateMachine(IStateMachineConfiguration stateMachineConfiguration, CancellationToken cancellationToken)
        {
            return await DefineStateMachine(stateMachineConfiguration, stateMachineConfiguration.MachineDefinition.MachineDefinitionId, cancellationToken);
        }

        public async Task<ICollection<IStateAction>> DefineStateActions(ICollection<IStateAction> stateActions, CancellationToken cancellationToken)
        {
            var command = CommandManager.Instance
                .DefineCommand<IStateAction>(
                    "INSERT INTO StateActions VALUES (@MachineDefinitionId, @StateName, @PurposeName, @TriggerName, @StateActionDescription, @CodeElementId);\n\n" +
                    "SELECT * FROM StateActions WHERE MachineDefinitionId = @MachineDefinitionId AND StateName = @StateName AND PurposeName = @PurposeName;",
                    CommandType.Text)
                .SendNullValues()
                .DefineResults<StateAction>()
                .Realize();

            var newStateActions = Enumerable.Empty<IStateAction>();

            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                // Cannot await in a linq expression.
                foreach (var stateAction in stateActions)
                {
                    newStateActions = newStateActions.Union(
                        await command.ExecuteAsync(DatabaseManagerPool.DatabaseManager,
                            stateAction, cancellationToken));
                }

                scope.Complete();
            }

            return newStateActions.ToList();
        }
    }
}