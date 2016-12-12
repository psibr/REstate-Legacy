using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Engine.Repositories;
using REstate.Engine.Services;

namespace REstate.Engine
{
    public class REstateMachine
        : IStateMachine
    {
        private readonly IConnectorFactoryResolver _connectorFactoryResolver;
        private readonly IRepositoryContextFactory _repositoryContextFactory;
        private readonly DotGraphCartographer _cartographer;

        protected IDictionary<State, StateConfiguration> StateMappings { get; }

        public REstateMachine(
            IConnectorFactoryResolver connectorFactoryResolver,
            IRepositoryContextFactory repositoryContextFactory,
            DotGraphCartographer cartographer,
            string apiKey,
            string machineName,
            string machineInstanceId,
            IDictionary<State, StateConfiguration> stateMappings)
        {
            _connectorFactoryResolver = connectorFactoryResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;

            ApiKey = apiKey;
            MachineInstanceId = machineInstanceId;
            MachineDefinitionId = machineName;

            StateMappings = stateMappings;
        }

        protected string ApiKey { get; }
        public string MachineInstanceId { get; }
        public string MachineDefinitionId { get; }

        public async Task FireAsync(
            Trigger trigger, 
            string contentType, string payload, 
            CancellationToken cancellationToken)
        {
            await FireAsync(trigger, contentType, payload, null, cancellationToken).ConfigureAwait(false);
        }

        public async Task FireAsync(
            Trigger trigger,
            string contentType, string payload, string lastCommitTag,
            CancellationToken cancellationToken)
        {
            using (var dataContext = _repositoryContextFactory.OpenContext(ApiKey))
            {
                var record = await dataContext.MachineInstances
                    .GetInstanceState(MachineInstanceId, cancellationToken).ConfigureAwait(false);

                var currentState = new State(MachineDefinitionId, record.StateName, record.CommitTag);

                var stateConfig = StateMappings[currentState];

                var transition = stateConfig.Transitions.Single(t => t.TriggerName == trigger.TriggerName);

                if (transition.Guard != null)
                {
                    var guardConnector = await _connectorFactoryResolver
                        .ResolveConnectorFactory(transition.Guard.ConnectorKey)
                        .BuildConnector(ApiKey).ConfigureAwait(false);

                    if (!await guardConnector
                        .ConstructPredicate(this, transition.Guard.Configuration)
                        .Invoke(cancellationToken).ConfigureAwait(false))
                    {
                        throw new InvalidOperationException("Guard clause prevented transition.");
                    }
                }

                record = await dataContext.MachineInstances.SetInstanceState(
                    MachineInstanceId,
                    currentState.StateName, 
                    trigger.TriggerName, 
                    lastCommitTag ?? currentState.CommitTag, 
                    CancellationToken.None).ConfigureAwait(false);

                currentState = new State(MachineDefinitionId, record.StateName, record.CommitTag);

                stateConfig = StateMappings[currentState];

                if(stateConfig.OnEntry != null)
                {
                    try
                    {
                        var entryConnector = await _connectorFactoryResolver
                            .ResolveConnectorFactory(stateConfig.OnEntry.ConnectorKey)
                            .BuildConnector(ApiKey).ConfigureAwait(false);
                        
                        await entryConnector
                            .ConstructAction(this, currentState, contentType, payload, stateConfig.OnEntry.Configuration) 
                            .Invoke(cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        if(stateConfig.OnEntry.FailureTransition != null)
                        {
                            await FireAsync(
                                new Trigger(MachineDefinitionId, stateConfig.OnEntry.FailureTransition.TriggerName), 
                                contentType, 
                                payload, 
                                currentState.CommitTag, 
                                cancellationToken).ConfigureAwait(false);
                        }

                        throw;
                    }
                }
            }
        }

        public async Task<bool> IsInStateAsync(State state, CancellationToken cancellationToken)
        {
            var currentState = await GetCurrentStateAsync(cancellationToken);

            //If exact match, true
            if(state == currentState)
                return true;

            var configuration = StateMappings[currentState];

            //Recursively look for anscestors
            while(configuration.ParentStateName != null)
            {
                //If state matches an ancestor, true
                if(configuration.ParentStateName == state.StateName) 
                    return true;
                
                //No match, move one level up the tree
                configuration = StateMappings[new State(MachineDefinitionId, configuration.ParentStateName)];
            }

            return false;
        }

        public async Task<State> GetCurrentStateAsync(CancellationToken cancellationToken)
        {
            State currentState;

            using (var dataContext = _repositoryContextFactory.OpenContext(ApiKey))
            {
                var record = await dataContext.MachineInstances
                    .GetInstanceState(MachineInstanceId, cancellationToken).ConfigureAwait(false);

                currentState = new State(MachineDefinitionId, record.StateName, record.CommitTag);
            }

            return currentState;
        }

        public async Task<ICollection<Trigger>> GetPermittedTriggersAsync(CancellationToken cancellationToken)
        {
            var currentState = await GetCurrentStateAsync(cancellationToken).ConfigureAwait(false);

            var configuration = StateMappings[currentState];

            return configuration.Transitions.Select(t => new Trigger(MachineDefinitionId, t.TriggerName)).ToList();
        }

        public override string ToString()
        {
            return _cartographer.WriteMap(StateMappings);
        }
    }
}
