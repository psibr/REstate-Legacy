using REstate.Configuration;
using REstate.Services;
using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Psibr.Platform.Logging;
using REstate.Repositories.Configuration;

namespace REstate.Stateless
{
    public class StatelessStateMachineFactory
        : IStateMachineFactory
    {
        private readonly IConnectorFactoryResolver _connectorFactoryResolver;
        public IPlatformLogger Logger { get; }

        public StatelessStateMachineFactory(
            IConnectorFactoryResolver connectorFactoryResolver, IPlatformLogger logger)
        {
            _connectorFactoryResolver = connectorFactoryResolver;
            Logger = logger;
        }

        public IStateMachine ConstructFromConfiguration(string apiKey, string machineInstanceId,
            Machine configuration,
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            var accessorMutator = new PersistentStateAccessorMutator(configurationRepositoryContextFactory, apiKey, machineInstanceId);

            return ConstructFromConfiguration(apiKey, accessorMutator, configuration);
        }

        public IStateMachine ConstructFromConfiguration(string apiKey, Machine configuration)
        {
            var accessorMutator = new InMemoryStateAccessorMutator();

            return ConstructFromConfiguration(apiKey, accessorMutator, configuration);
        }

        protected IStateMachine ConstructFromConfiguration(string apiKey, IStateAccessorMutator accessorMutator,
            Machine configuration)
        {
            var machine = new StateMachine<State, Trigger>(accessorMutator.Accessor, accessorMutator.Mutator);

            var stateMachine = new StatelessStateMachineAdapter(machine,
                configuration.MachineName, accessorMutator.MachineInstanceId);

            if (configuration.AutoIgnoreTriggers)
                machine.OnUnhandledTrigger((i, i1) => { /* ignore */ });

            foreach (var trigger in configuration.StateConfigurations
                .SelectMany(sc => sc.Transitions ?? new Transition[0])
                .Select(tr => tr.TriggerName)
                .Distinct())
            {
                machine.SetTriggerParameters<string>(new Trigger(configuration.MachineName, trigger));
            }

            foreach (var stateConfiguration in configuration.StateConfigurations)
            {
                var stateSettings = machine.Configure(new State(configuration.MachineName, stateConfiguration.StateName));


                    if (stateConfiguration.OnEntry != null)
                    {
                        ConfigureOnEntryAction(apiKey, stateMachine, configuration, stateConfiguration, stateSettings);
                    }

                    if (stateConfiguration.OnEntryFrom != null)
                    {
                        foreach (var onEntryFromAction in stateConfiguration.OnEntryFrom)
                        {
                            ConfigureOnEntryFromAction(apiKey, stateMachine, configuration, onEntryFromAction,
                                stateSettings);
                        }
                        
                    }

                    if (stateConfiguration.OnExit != null)
                    {
                        ConfigureOnExitAction(apiKey, stateMachine, configuration, stateConfiguration, stateSettings);
                    }


                //Configure as substate if needed
                if (stateConfiguration.ParentStateName != null)
                    stateSettings.SubstateOf(new State(configuration.MachineName, stateConfiguration.ParentStateName));

                foreach (var transition in stateConfiguration.Transitions ?? new Transition[0])
                {

                    if (transition.Guard == null)
                        if (stateConfiguration.StateName != transition.ResultantStateName)
                            stateSettings.Permit(new Trigger(configuration.MachineName, transition.TriggerName),
                                new State(configuration.MachineName, transition.ResultantStateName));
                        else
                            stateSettings.PermitReentry(new Trigger(configuration.MachineName, transition.TriggerName));
                    else
                    {
                        //Retrieve guard definition and construct
                        var guardDefinition = transition.Guard;
                        var guard = CreateGuardClause(apiKey, stateMachine, configuration, guardDefinition);

                        if (stateConfiguration.StateName != transition.ResultantStateName)
                            stateSettings.PermitIf(new Trigger(configuration.MachineName, transition.TriggerName),
                                new State(configuration.MachineName, transition.ResultantStateName), guard, guardDefinition.Name);
                        else
                            stateSettings.PermitReentryIf(new Trigger(
                                configuration.MachineName, transition.TriggerName), guard, guardDefinition.Name);
                    }
                }

                foreach (var trigger in stateConfiguration.IgnoreRules ?? new string[0])
                {
                    stateSettings.Ignore(new Trigger(configuration.MachineName, trigger));
                }
            }

            return stateMachine;
        }


        protected Func<bool> CreateGuardClause(string apiKey, IStateMachine stateMachine, Machine configuration, Code guardDefinition)
        {
            var hostFactory = _connectorFactoryResolver.ResolveConnectorFactory(guardDefinition.ConnectorKey);
            Func<bool> guard = () =>
                hostFactory.BuildConnector(apiKey).Result
                    .ConstructPredicate(stateMachine, guardDefinition)
                    .Invoke(CancellationToken.None).Result;

            return guard;
        }

        protected void ConfigureOnExitAction(string apiKey,
            IStateMachine stateMachine, Machine configuration,
            StateConfiguration stateConfiguration,
            StateMachine<State, Trigger>.StateConfiguration stateSettings)
        {
            var hostFactory = _connectorFactoryResolver.ResolveConnectorFactory(stateConfiguration.OnExit.ConnectorKey);
            stateSettings.OnExit(() =>
                hostFactory.BuildConnector(apiKey).Result
                    .ConstructAction(stateMachine, stateConfiguration.OnExit)
                    .Invoke(CancellationToken.None)
            , stateConfiguration.OnExit.Description);
        }

        protected void ConfigureOnEntryFromAction(string apiKey,
            IStateMachine stateMachine, Machine configuration,
            OnEntryFrom onEntryFrom,
            StateMachine<State, Trigger>.StateConfiguration stateSettings)
        {
            var hostFactory = _connectorFactoryResolver.ResolveConnectorFactory(onEntryFrom.ConnectorKey);
            stateSettings.OnEntryFrom(
                new StateMachine<State, Trigger>.TriggerWithParameters<string>(
                    new Trigger(configuration.MachineName, onEntryFrom.FromTrigger)), payload =>
                        hostFactory.BuildConnector(apiKey).Result
                            .ConstructAction(stateMachine, payload, onEntryFrom)
                            .Invoke(CancellationToken.None),
                onEntryFrom.Description);

        }

        protected void ConfigureOnEntryAction(string apiKey,
            IStateMachine stateMachine, Machine configuration,
            StateConfiguration stateConfiguration,
            StateMachine<State, Trigger>.StateConfiguration stateSettings)
        {
            var hostFactory = _connectorFactoryResolver.ResolveConnectorFactory(stateConfiguration.OnEntry.ConnectorKey);
            stateSettings.OnEntry(() =>
                hostFactory.BuildConnector(apiKey).Result
                    .ConstructAction(stateMachine, stateConfiguration.OnEntry)
                    .Invoke(CancellationToken.None),
                stateConfiguration.OnEntry.Description);
        }

        protected interface IStateAccessorMutator
        {
            string MachineInstanceId { get; }

            State Accessor();

            void Mutator(Trigger trigger, State state);
        }

        protected class PersistentStateAccessorMutator : IStateAccessorMutator
        {
            private InstanceRecord _lastState;
            private readonly IConfigurationRepository _context;

            public PersistentStateAccessorMutator(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
                string apiKey, string machineInstanceId)
            {
                MachineInstanceId = machineInstanceId;

                _context = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(apiKey);
            }

            public string MachineInstanceId { get; }

            public State Accessor()
            {
                _lastState = _context.MachineInstances.GetInstanceState(MachineInstanceId);

                return new  State(_lastState.MachineName, _lastState.StateName);
            }

            public void Mutator(Trigger trigger, State state)
            {
                _context.MachineInstances.SetInstanceState(MachineInstanceId,
                    state.StateName, trigger.TriggerName, _lastState.CommitTag);
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

        public void Mutator(Trigger trigger, State state)
        {
            _state = state;
        }
    }
}
}