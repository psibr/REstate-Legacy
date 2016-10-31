﻿using REstate.Configuration;
using REstate.Engine.Repositories;
using REstate.Engine.Services;
using REstate.Logging;
using Stateless;
using System;
using System.Linq;
using System.Threading;

namespace REstate.Engine.Stateless
{
    public class StatelessStateMachineFactory
        : IStateMachineFactory
    {
        protected IConnectorFactoryResolver ConnectorFactoryResolver { get; }
        protected IPlatformLogger Logger { get; }

        private readonly IRepositoryContextFactory _RepositoryContextFactory;

        public StatelessStateMachineFactory(
            IConnectorFactoryResolver connectorFactoryResolver, IRepositoryContextFactory repositoryContextFactory, IPlatformLogger logger)
        {
            ConnectorFactoryResolver = connectorFactoryResolver;

            _RepositoryContextFactory = repositoryContextFactory;

            Logger = logger;
        }

        public IStateMachine ConstructFromConfiguration(string apiKey, string machineInstanceId,
            Machine configuration)
        {
            var accessorMutator = new PersistentStateAccessorMutator(_RepositoryContextFactory, apiKey, machineInstanceId);

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
                    ConfigureOnEntryAction(apiKey, stateMachine, configuration, stateConfiguration, accessorMutator, stateSettings);
                }

                if (stateConfiguration.OnEntryFrom != null)
                {
                    foreach (var onEntryFromAction in stateConfiguration.OnEntryFrom)
                    {
                        ConfigureOnEntryFromAction(apiKey, stateMachine, configuration, stateConfiguration, accessorMutator, onEntryFromAction,
                            stateSettings);
                    }
                }

                if (stateConfiguration.OnExit != null)
                {
                    ConfigureOnExitAction(apiKey, stateMachine, configuration, stateConfiguration, accessorMutator, stateSettings);
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
            var hostFactory = ConnectorFactoryResolver.ResolveConnectorFactory(guardDefinition.ConnectorKey);
            Func<bool> guard = () =>
                hostFactory.BuildConnector(apiKey).Result
                    .ConstructPredicate(stateMachine, guardDefinition.Configuration)
                    .Invoke(CancellationToken.None).Result;

            return guard;
        }

        protected void ConfigureOnExitAction(string apiKey,
            IStateMachine stateMachine, Machine configuration,
            StateConfiguration stateConfiguration, IStateAccessorMutator accessorMutator,
            StateMachine<State, Trigger>.StateConfiguration stateSettings)
        {
            var hostFactory = ConnectorFactoryResolver.ResolveConnectorFactory(stateConfiguration.OnExit.ConnectorKey);
            stateSettings.OnExit(() =>
                hostFactory.BuildConnector(apiKey).Result
                    .ConstructAction(stateMachine, accessorMutator.CurrentState, stateConfiguration.OnExit.Configuration)
                    .Invoke(CancellationToken.None).Wait()
            , stateConfiguration.OnExit.Description);
        }

        protected void ConfigureOnEntryFromAction(string apiKey,
            IStateMachine stateMachine, Machine configuration, StateConfiguration stateConfiguration, IStateAccessorMutator accessorMutator,
            OnEntryFrom onEntryFrom,
            StateMachine<State, Trigger>.StateConfiguration stateSettings)
        {
            var hostFactory = ConnectorFactoryResolver.ResolveConnectorFactory(onEntryFrom.ConnectorKey);
            stateSettings.OnEntryFrom(
                new StateMachine<State, Trigger>.TriggerWithParameters<string>(
                    new Trigger(configuration.MachineName, onEntryFrom.FromTrigger)), payload =>
                        hostFactory.BuildConnector(apiKey).Result
                            .ConstructAction(stateMachine, accessorMutator.CurrentState, payload, onEntryFrom.Configuration)
                            .Invoke(CancellationToken.None).Wait(),
                onEntryFrom.Description);
        }

        protected void ConfigureOnEntryAction(string apiKey,
            IStateMachine stateMachine, Machine configuration,
            StateConfiguration stateConfiguration, IStateAccessorMutator accessorMutator,
            StateMachine<State, Trigger>.StateConfiguration stateSettings)
        {
            var hostFactory = ConnectorFactoryResolver.ResolveConnectorFactory(stateConfiguration.OnEntry.ConnectorKey);
            stateSettings.OnEntry(() =>
                hostFactory.BuildConnector(apiKey).Result
                    .ConstructAction(stateMachine, accessorMutator.CurrentState, stateConfiguration.OnEntry.Configuration)
                    .Invoke(CancellationToken.None).Wait(),
                stateConfiguration.OnEntry.Description);
        }

        protected interface IStateAccessorMutator
        {
            string MachineInstanceId { get; }

            State Accessor();

            void Mutator(State state);

            State CurrentState { get; }
        }

        protected class PersistentStateAccessorMutator : IStateAccessorMutator
        {
            private InstanceRecord _lastState;
            private readonly IEngineRepositoryContext _context;

            public State CurrentState { get { return new State(_lastState.MachineName, _lastState.StateName, _lastState.CommitTag); }  }

            public PersistentStateAccessorMutator(IRepositoryContextFactory configurationRepositoryContextFactory,
                string apiKey, string machineInstanceId)
            {
                MachineInstanceId = machineInstanceId;

                _context = configurationRepositoryContextFactory.OpenContext(apiKey);
            }

            public string MachineInstanceId { get; }

            public State Accessor()
            {
                var state = _context.MachineInstances.GetInstanceState(MachineInstanceId, CancellationToken.None).Result;
                _lastState = state;

                return new State(state.MachineName, state.StateName, state.CommitTag);
            }

            public void Mutator(State state)
            {
                _lastState = _context.MachineInstances.SetInstanceState(MachineInstanceId,
                    state.StateName, null, _lastState.CommitTag, CancellationToken.None).Result;
            }

            ~PersistentStateAccessorMutator()
            {
                _context.Dispose();
            }
        }

        protected class InMemoryStateAccessorMutator
            : IStateAccessorMutator
        {

            public State CurrentState { get { return new State(_state.MachineDefinitionId, _state.StateName, _state.CommitTag); } }

            private State _state;

            public string MachineInstanceId
                => Guid.NewGuid().ToString();

            public State Accessor()
            {
                return _state;
            }

            public void Mutator(State state)
            {
                _state = new State(state.MachineDefinitionId, state.StateName, Guid.NewGuid().ToString());
            }
        }
    }
}
