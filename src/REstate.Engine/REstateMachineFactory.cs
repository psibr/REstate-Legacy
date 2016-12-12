using System;
using System.Collections.Generic;
using System.Linq;
using REstate.Configuration;
using REstate.Engine.Repositories;
using REstate.Engine.Services;

namespace REstate.Engine
{
    public class REstateMachineFactory : IStateMachineFactory
    {
        private IConnectorFactoryResolver _connectorFactoryResolver;
        private IRepositoryContextFactory _repositoryContextFactory;
        private readonly DotGraphCartographer _cartographer;

        public REstateMachineFactory(
            IConnectorFactoryResolver connectorFactoryResolver,
            IRepositoryContextFactory repositoryContextFactory,
            DotGraphCartographer cartographer)
        {
            _connectorFactoryResolver = connectorFactoryResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;
        }

        public IStateMachine ConstructFromConfiguration(string apiKey, string machineInstanceId, Machine configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var stateMappings = configuration.StateConfigurations
                .ToDictionary(stateConfig => new State(configuration.MachineName, stateConfig.StateName), stateConfig => stateConfig);

            foreach (var serviceState in configuration.ServiceStates ?? new ServiceState[0])
            {
                var parentStateConfiguration = new StateConfiguration
                {
                    StateName = serviceState.StateName,
                    ParentStateName = null,
                    StateDescription = serviceState.Description,
                    OnEntry = new EntryConnector
                    {  
                        ConnectorKey = serviceState.OnEntry.ConnectorKey,
                        Configuration = serviceState.OnEntry.Configuration,
                        Description = serviceState.OnEntry.Description,
                        FailureTransition = new ExceptionTransition
                        {
                            TriggerName = "Fail"
                        }
                    },
                    Transitions = new[]
                    {
                        new Transition
                        {
                            TriggerName = "Fail",
                            ResultantStateName = $"{serviceState.StateName}Failed"
                        }
                    }
                };

                stateMappings.Add(new State(configuration.MachineName, parentStateConfiguration.StateName), parentStateConfiguration);

                if (!serviceState.DisableAcknowledgement)
                {
                    // Add a transition from parent to accepting and rejecting states.
                    parentStateConfiguration.Transitions = new[]
                    {
                        parentStateConfiguration.Transitions[0],
                        new Transition
                        {
                            TriggerName = "Accept",
                            ResultantStateName = $"{serviceState.StateName}Accepted"
                        },
                        new Transition
                        {
                            TriggerName = "Reject",
                            ResultantStateName = $"{serviceState.StateName}Rejected"
                        }
                    };

                    var acceptedStateConfiguration = new StateConfiguration
                    {
                        StateName = $"{serviceState.StateName}Accepted",
                        ParentStateName = serviceState.StateName,
                        StateDescription = "Action was accepted for processing.",
                        Transitions = new[]
                        {
                            new Transition
                            {
                                TriggerName = "Reject",
                                ResultantStateName = $"{serviceState.StateName}Rejected"
                            },
                            new Transition
                            {
                                TriggerName = "Fail",
                                ResultantStateName = $"{serviceState.StateName}Failed"
                            }
                        // Acknowledge states will be used, add outbound transitions to the accepting state.
                        }.Concat(serviceState.Transitions).ToArray()
                    };

                    var rejectedStateConfiguration = new StateConfiguration
                    {
                        StateName = $"{serviceState.StateName}Rejected",
                        ParentStateName = serviceState.StateName,
                        StateDescription = "Action was rejected.",
                        Transitions = null // Terminal state.
                    };

                    stateMappings.Add(new State(configuration.MachineName, acceptedStateConfiguration.StateName), acceptedStateConfiguration);
                    stateMappings.Add(new State(configuration.MachineName, rejectedStateConfiguration.StateName), rejectedStateConfiguration);
                }
                else
                {
                    // Acknowledge states won't be used, add outbound transitions to the parent state.
                    parentStateConfiguration.Transitions = parentStateConfiguration
                        .Transitions
                        .Concat(serviceState.Transitions)
                        .ToArray();
                }

                var failureStateConfiguration = new StateConfiguration
                {
                    StateName = $"{serviceState.StateName}Failed",
                    ParentStateName = serviceState.StateName,
                    StateDescription = "Failed to process.",
                    Transitions = new[]
                    {
                        new Transition
                        {
                            TriggerName = "Retry",
                            ResultantStateName = serviceState.StateName
                        }
                    },
                    OnEntry = serviceState.RetryDelaySeconds == null || serviceState.RetryDelaySeconds <= 0
                        ? null
                        : new EntryConnector
                        {
                            ConnectorKey = "Delay",
                            Description = $"Retry after {serviceState.RetryDelaySeconds} seconds.",
                            Configuration = new Dictionary<string, string>
                            {
                                ["machineInstanceId"] = machineInstanceId,
                                ["triggerName"] = "Retry",
                                ["delay"] = serviceState.RetryDelaySeconds.ToString(),
                                ["verifyCommitTag"] = "true"
                            }
                        }
                };

                stateMappings.Add(new State(configuration.MachineName, failureStateConfiguration.StateName), failureStateConfiguration);
            }

            var reStateMachine = new REstateMachine(_connectorFactoryResolver, _repositoryContextFactory, _cartographer, apiKey, configuration.MachineName, machineInstanceId, stateMappings);

            return reStateMachine;
        }
    }
}