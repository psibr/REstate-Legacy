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
            string machineId,
            IDictionary<State, StateConfiguration> stateMappings)
        {
            _connectorFactoryResolver = connectorFactoryResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;

            ApiKey = apiKey;
            MachineId = machineId;

            StateMappings = stateMappings;
        }

        protected string ApiKey { get; }
        public string MachineId { get; }

        public Task<InstanceRecord> FireAsync(
            Trigger trigger, 
            string contentType, string payload, 
            CancellationToken cancellationToken)
        {
            return FireAsync(trigger, contentType, payload, null, cancellationToken);
        }

        public async Task<InstanceRecord> FireAsync(
            Trigger trigger,
            string contentType, string payload, Guid? lastCommitTag,
            CancellationToken cancellationToken)
        {
            using (var dataContext = _repositoryContextFactory.OpenContext(ApiKey))
            {
                var currentState = await dataContext.Machines
                    .GetMachineStateAsync(MachineId, cancellationToken).ConfigureAwait(false);

                var stateConfig = StateMappings[currentState];

                var transition = stateConfig.Transitions.Single(t => t.TriggerName == trigger.TriggerName);

                if (transition.Guard != null)
                {
                    var guardConnector = await _connectorFactoryResolver
                        .ResolveConnectorFactory(transition.Guard.ConnectorKey)
                        .BuildConnectorAsync(ApiKey, cancellationToken).ConfigureAwait(false);

                    if (!await guardConnector
                        .ConstructPredicate(this, transition.Guard.Configuration)
                        .Invoke(cancellationToken).ConfigureAwait(false))
                    {
                        throw new InvalidOperationException("Guard clause prevented transition.");
                    }
                }

                currentState = await dataContext.Machines.SetMachineStateAsync(
                    MachineId,
                    transition.ResultantStateName, 
                    trigger.TriggerName, 
                    lastCommitTag ?? Guid.Parse(currentState.CommitTag), 
                    cancellationToken).ConfigureAwait(false);

                stateConfig = StateMappings[currentState];

                if(stateConfig.OnEntry != null)
                {
                    try
                    {
                        var entryConnector = await _connectorFactoryResolver
                            .ResolveConnectorFactory(stateConfig.OnEntry.ConnectorKey)
                            .BuildConnectorAsync(ApiKey, cancellationToken).ConfigureAwait(false);
                        
                        await entryConnector
                            .ConstructAction(this, currentState, contentType, payload, stateConfig.OnEntry.Configuration) 
                            .Invoke(cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        if(stateConfig.OnEntry.FailureTransition != null)
                        {
                            await FireAsync(
                                new Trigger(stateConfig.OnEntry.FailureTransition.TriggerName), 
                                contentType, 
                                payload, 
                                Guid.Parse(currentState.CommitTag), 
                                cancellationToken).ConfigureAwait(false);
                        }

                        throw;
                    }
                }

                return currentState;
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
                configuration = StateMappings[new State(configuration.ParentStateName)];
            }

            return false;
        }

        public async Task<InstanceRecord> GetCurrentStateAsync(CancellationToken cancellationToken)
        {
            InstanceRecord currentState;

            using (var dataContext = _repositoryContextFactory.OpenContext(ApiKey))
            {
                currentState = await dataContext.Machines
                    .GetMachineStateAsync(MachineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return currentState;
        }

        public async Task<ICollection<Trigger>> GetPermittedTriggersAsync(CancellationToken cancellationToken)
        {
            var currentState = await GetCurrentStateAsync(cancellationToken).ConfigureAwait(false);

            var configuration = StateMappings[currentState];

            return configuration.Transitions.Select(t => new Trigger(t.TriggerName)).ToList();
        }

        public override string ToString()
        {
            return _cartographer.WriteMap(StateMappings);
        }
    }
}
