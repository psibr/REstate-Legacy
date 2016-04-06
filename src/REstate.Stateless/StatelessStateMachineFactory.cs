using REstate.Configuration;
using REstate.Repositories.Instances;
using REstate.Services;
using Stateless;
using System;
using System.Linq;
using System.Threading;

namespace REstate.Stateless
{
    public class StatelessStateMachineFactory
        : IStateMachineFactory
    {
        private readonly IConnectorFactoryResolver _connectorFactoryResolver;

        public StatelessStateMachineFactory(
            IConnectorFactoryResolver connectorFactoryResolver)
        {
            _connectorFactoryResolver = connectorFactoryResolver;
        }

        public IStateMachine ConstructFromConfiguration(string apiKey, string machineInstanceId,
            IStateMachineConfiguration configuration,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory)
        {
            var accessorMutator = new PersistentStateAccessorMutator(instanceRepositoryContextFactory, apiKey, machineInstanceId);

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

            var stateMachine = new StatelessStateMachineAdapter(machine,
                configuration.MachineDefinition.MachineDefinitionId, accessorMutator.MachineInstanceId);

            if (configuration.MachineDefinition.AutoIgnoreNotConfiguredTriggers)
                machine.OnUnhandledTrigger((i, i1) => { /* ignore */ });

            foreach (var trigger in configuration.Triggers)
            {
                machine.SetTriggerParameters<string>(new Trigger(trigger.MachineDefinitionId, trigger.TriggerName));
            }

            foreach (var stateConfiguration in configuration.StateConfigurations)
            {
                var stateSettings = machine.Configure(new State(stateConfiguration.State.MachineDefinitionId, stateConfiguration.State.StateName));

                if (configuration.CodeElements != null)
                {

                    if (stateConfiguration.OnEntryAction != null)
                    {
                        ConfigureOnEntryAction(apiKey, stateMachine, configuration, stateConfiguration, stateSettings);
                    }

                    if (stateConfiguration.OnEntryFromAction != null)
                    {
                        ConfigureOnEntryFromAction(apiKey, stateMachine, configuration, stateConfiguration,
                            stateSettings);
                    }

                    if (stateConfiguration.OnExitAction != null)
                    {
                        ConfigureOnExitAction(apiKey, stateMachine, configuration, stateConfiguration, stateSettings);
                    }
                }


                //Configure as substate if needed
                if (stateConfiguration.State.ParentStateName != null)
                    stateSettings.SubstateOf(new State(stateConfiguration.State.MachineDefinitionId, stateConfiguration.State.ParentStateName));

                foreach (var transition in stateConfiguration.Transitions)
                {

                    if (configuration.CodeElements == null || transition.GuardName == null)
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

            var hostFactory = _connectorFactoryResolver.ResolveConnectorFactory(codeElement.ConnectorKey);
            Func<bool> guard = () =>
                hostFactory.BuildConnector(apiKey).Result
                    .ConstructPredicate(stateMachine, codeElement)
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

            var hostFactory = _connectorFactoryResolver.ResolveConnectorFactory(codeElement.ConnectorKey);
            stateSettings.OnExit(() =>
                hostFactory.BuildConnector(apiKey).Result
                    .ConstructAction(stateMachine, codeElement)
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

            var hostFactory = _connectorFactoryResolver.ResolveConnectorFactory(codeElement.ConnectorKey);
            stateSettings.OnEntryFrom(
                new StateMachine<State, Trigger>.TriggerWithParameters<string>(
                    new Trigger(stateConfiguration.State.MachineDefinitionId, stateConfiguration.OnEntryFromAction.TriggerName)), payload =>
                        hostFactory.BuildConnector(apiKey).Result
                            .ConstructAction(stateMachine, payload, codeElement)
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

            var hostFactory = _connectorFactoryResolver.ResolveConnectorFactory(codeElement.ConnectorKey);
            stateSettings.OnEntry(() =>
                hostFactory.BuildConnector(apiKey).Result
                    .ConstructAction(stateMachine, codeElement)
                    .Invoke(CancellationToken.None),
                stateConfiguration.OnEntryAction.StateActionDescription ?? codeElement.CodeElementDescription);
        }

        protected interface IStateAccessorMutator
        {
            string MachineInstanceId { get; }

            State Accessor();

            void Mutator(State state);
        }

        protected class PersistentStateAccessorMutator : IStateAccessorMutator
        {
            private State _lastState;
            private readonly IInstanceRepository _context;

            public PersistentStateAccessorMutator(IInstanceRepositoryContextFactory instanceRepositoryContextFactory,
                string apiKey, string machineInstanceId)
            {
                MachineInstanceId = machineInstanceId;

                _context = instanceRepositoryContextFactory.OpenInstanceRepositoryContext(apiKey);
            }

            public string MachineInstanceId { get; }

            public State Accessor()
            {
                _lastState = _context.MachineInstances.GetInstanceState(MachineInstanceId);

                return _lastState;
            }

            public void Mutator(State state)
            {
                _context.MachineInstances.SetInstanceState(MachineInstanceId, state, _lastState);
            }

            ~PersistentStateAccessorMutator()
            {
                _context.Dispose();
            }

        
    }

    protected class InMemoryStateAccessorMutator
        : IStateAccessorMutator
    {
        private State _state;

        public string MachineInstanceId
            => Guid.NewGuid().ToString();

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