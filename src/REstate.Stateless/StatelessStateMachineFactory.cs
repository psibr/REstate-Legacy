using System;
using System.Linq;
using System.Threading;
using REstate.Configuration;
using REstate.Repositories;
using REstate.Services;
using Stateless;

namespace REstate.Stateless
{
    public class StatelessStateMachineFactory
        : IStateMachineFactory
    {
        private readonly IConfigurationRepositoryContextFactory _configurationRepositoryContextFactory;
        private readonly IInstanceRepositoryContextFactory _instanceRepositoryContextFactory;
        private readonly IScriptHostFactoryResolver _scriptHostFactoryResolver;

        public StatelessStateMachineFactory(
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory,
            IScriptHostFactoryResolver scriptHostFactoryResolver)
        {
            _configurationRepositoryContextFactory = configurationRepositoryContextFactory;
            _instanceRepositoryContextFactory = instanceRepositoryContextFactory;
            _scriptHostFactoryResolver = scriptHostFactoryResolver;
        }

        public IStateMachine ConstructFromConfiguration(string apiKey, Guid machineInstanceGuid, IStateMachineConfiguration configuration)
        {
            var accessorMutator = new PersistentStateAccessorMutator(_instanceRepositoryContextFactory, apiKey, machineInstanceGuid);

            return ConstructFromConfiguration(apiKey, accessorMutator, configuration);
        }

        public IStateMachine ConstructFromConfiguration(string apiKey, IStateMachineConfiguration configuration)
        {
            var accessorMutator = new InMemoryStateAccessorMutator();

            return ConstructFromConfiguration(apiKey, accessorMutator, configuration);
        }

        protected IStateMachine ConstructFromConfiguration(string apiKey, IStateAccessorMutator accessorMutator,
            IStateMachineConfiguration configuration)
        {
            var machine = new StateMachine<State, Trigger>(accessorMutator.Accessor, accessorMutator.Mutator);

            var stateMachine = new StatelessStateMachineAdapter(machine);

            if (configuration.MachineDefinition.AutoIgnoreNotConfiguredTriggers)
                machine.OnUnhandledTrigger((i, i1) => { /* ignore */ });

            foreach (var trigger in configuration.Triggers)
            {
                machine.SetTriggerParameters<string>(new Trigger(trigger.MachineDefinitionId, trigger.TriggerName));
            }

            foreach (var stateConfiguration in configuration.StateConfigurations)
            {
                var stateSettings = machine.Configure(new State(stateConfiguration.State.MachineDefinitionId, stateConfiguration.State.StateName));

                if (stateConfiguration.OnEntryAction != null)
                {
                    ConfigureOnEntryAction(apiKey, stateMachine, configuration, stateConfiguration, stateSettings);
                }

                if (stateConfiguration.OnEntryFromAction != null)
                {
                    ConfigureOnEntryFromAction(apiKey, stateMachine, configuration, stateConfiguration, stateSettings);
                }

                if (stateConfiguration.OnExitAction != null)
                {
                    ConfigureOnExitAction(apiKey, stateMachine, configuration, stateConfiguration, stateSettings);
                }


                //Configure as substate if needed
                if (stateConfiguration.State.ParentStateName != null)
                    stateSettings.SubstateOf(new State(stateConfiguration.State.MachineDefinitionId, stateConfiguration.State.ParentStateName));

                foreach (var transition in stateConfiguration.Transitions)
                {

                    if (transition.GuardName == null)
                        if (transition.StateName != transition.ResultantStateName)
                            stateSettings.Permit(new Trigger(transition.MachineDefinitionId, transition.TriggerName),
                                new State(transition.MachineDefinitionId, transition.ResultantStateName));
                        else
                            stateSettings.PermitReentry(new Trigger(transition.MachineDefinitionId, transition.TriggerName));
                    else
                    {
                        //Retrieve guard definition and construct
                        var guardDefinition = configuration.Guards.Single(d => d.GuardName == transition.GuardName);
                        var guard = CreateGuardClause(apiKey, stateMachine, configuration, guardDefinition);

                        if (transition.StateName != transition.ResultantStateName)
                            stateSettings.PermitIf(new Trigger(transition.MachineDefinitionId, transition.TriggerName),
                                new State(transition.MachineDefinitionId, transition.ResultantStateName), guard, guardDefinition.GuardName);
                        else
                            stateSettings.PermitReentryIf(new Trigger(transition.MachineDefinitionId, transition.TriggerName), guard, guardDefinition.GuardName);
                    }
                }

                foreach (var ignoreRule in stateConfiguration.IgnoreRules)
                {
                    stateSettings.Ignore(new Trigger(ignoreRule.MachineDefinitionId, ignoreRule.TriggerName));
                }
            }

            return stateMachine;
        }


        protected Func<bool> CreateGuardClause(string apiKey, IStateMachine stateMachine, IStateMachineConfiguration configuration, IGuard guardDefinition)
        {
            if (guardDefinition.CodeElementId == null) return () => false; //No code found, assume this is an unsafe operation.

            var codeElement = configuration.CodeElements.SingleOrDefault(ce =>
                ce.CodeElementId == guardDefinition.CodeElementId);

            if (codeElement == null) throw new ArgumentException($"CodeElement with Id {guardDefinition.CodeElementId} was not found.");

            var hostFactory = _scriptHostFactoryResolver.ResolveScriptHostFactory(codeElement.CodeTypeId);
            Func<bool> guard = () =>
                hostFactory.BuildScriptHost(apiKey).Result
                    .BuildAsyncPredicateScript(stateMachine, codeElement)
                    .Invoke(CancellationToken.None).Result;

            return guard;
        }

        protected void ConfigureOnExitAction(string apiKey,
            IStateMachine stateMachine, IStateMachineConfiguration configuration,
            IStateConfiguration stateConfiguration,
            StateMachine<State, Trigger>.StateConfiguration stateSettings)
        {
            if (stateConfiguration.OnExitAction.CodeElementId == null) return;
            var codeElement = configuration.CodeElements.SingleOrDefault(ce =>
                ce.CodeElementId == stateConfiguration.OnExitAction.CodeElementId);

            if (codeElement == null) throw new ArgumentException($"CodeElement with Id {stateConfiguration.OnExitAction.CodeElementId} was not found.");

            var hostFactory = _scriptHostFactoryResolver.ResolveScriptHostFactory(codeElement.CodeTypeId);
            stateSettings.OnExit(() =>
                hostFactory.BuildScriptHost(apiKey).Result
                    .BuildAsyncActionScript(stateMachine, codeElement)
                    .Invoke(CancellationToken.None)
            , stateConfiguration.OnExitAction.StateActionDescription ?? codeElement.CodeElementDescription);
        }

        protected void ConfigureOnEntryFromAction(string apiKey,
            IStateMachine stateMachine, IStateMachineConfiguration configuration,
            IStateConfiguration stateConfiguration,
            StateMachine<State, Trigger>.StateConfiguration stateSettings)
        {
            if (stateConfiguration.OnEntryFromAction.CodeElementId == null) return;
            var codeElement = configuration.CodeElements.SingleOrDefault(ce =>
                ce.CodeElementId == stateConfiguration.OnEntryFromAction.CodeElementId);

            if (codeElement == null) throw new ArgumentException($"CodeElement with Id {stateConfiguration.OnEntryFromAction.CodeElementId} was not found.");

            var hostFactory = _scriptHostFactoryResolver.ResolveScriptHostFactory(codeElement.CodeTypeId);
            stateSettings.OnEntryFrom(
                new StateMachine<State, Trigger>.TriggerWithParameters<string>(
                    new Trigger(stateConfiguration.State.MachineDefinitionId, stateConfiguration.OnEntryFromAction.TriggerName)), payload =>
                        hostFactory.BuildScriptHost(apiKey).Result
                            .BuildAsyncActionScript(stateMachine, payload, codeElement)
                            .Invoke(CancellationToken.None),
                stateConfiguration.OnEntryFromAction.StateActionDescription ?? codeElement.CodeElementDescription);

        }

        protected void ConfigureOnEntryAction(string apiKey,
            IStateMachine stateMachine, IStateMachineConfiguration configuration,
            IStateConfiguration stateConfiguration,
            StateMachine<State, Trigger>.StateConfiguration stateSettings)
        {
            if (stateConfiguration.OnEntryAction.CodeElementId == null) return;
            var codeElement = configuration.CodeElements.SingleOrDefault(ce =>
                ce.CodeElementId == stateConfiguration.OnEntryAction.CodeElementId);

            if (codeElement == null) throw new ArgumentException($"CodeElement with Id {stateConfiguration.OnEntryFromAction.CodeElementId} was not found.");

            var hostFactory = _scriptHostFactoryResolver.ResolveScriptHostFactory(codeElement.CodeTypeId);
            stateSettings.OnEntry(() =>
                hostFactory.BuildScriptHost(apiKey).Result
                    .BuildAsyncActionScript(stateMachine, codeElement)
                    .Invoke(CancellationToken.None),
                stateConfiguration.OnEntryAction.StateActionDescription ?? codeElement.CodeElementDescription);
        }

        protected interface IStateAccessorMutator
        {
            State Accessor();
            void Mutator(State state);
        }

        protected class PersistentStateAccessorMutator : IStateAccessorMutator
        {
            private readonly IInstanceRepositoryContextFactory _instanceRepositoryContextFactory;
            private readonly string _apiKey;
            private readonly Guid _machineInstanceGuid;

            public PersistentStateAccessorMutator(IInstanceRepositoryContextFactory instanceRepositoryContextFactory, string apiKey, Guid machineInstanceGuid)
            {
                _instanceRepositoryContextFactory = instanceRepositoryContextFactory;
                _apiKey = apiKey;
                _machineInstanceGuid = machineInstanceGuid;
            }

            public State Accessor()
            {
                using (var repository = _instanceRepositoryContextFactory.OpenInstanceRepositoryContext(_apiKey))
                {
                    return repository.MachineInstances.GetInstanceState(_machineInstanceGuid, CancellationToken.None).Result;
                }
            }

            public void Mutator(State state)
            {
                using (var repository = _instanceRepositoryContextFactory.OpenInstanceRepositoryContext(_apiKey))
                {
                    repository.MachineInstances.SetInstanceState(_machineInstanceGuid, state, CancellationToken.None);
                }
            }
        }

        protected class InMemoryStateAccessorMutator
            : IStateAccessorMutator
        {
            private State _state;
            public State Accessor()
            {
                return _state;
            }

            public void Mutator(State state)
            {
                _state = state;
            }
        }
    }
}