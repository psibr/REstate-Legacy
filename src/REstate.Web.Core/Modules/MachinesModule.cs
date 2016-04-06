using System;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Modules;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using REstate.Repositories.Instances;
using REstate.Services;
using REstate.Web.Core.Requests;

namespace REstate.Web.Core.Modules
{
    /// <summary>
    /// Machine interactions module.
    /// </summary>
    public class MachinesModule
        : SecuredModule
    {
        /// <summary>
        /// Registers routes for interacting with machines.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        /// <param name="instanceRepositoryContextFactory">The instance repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        public MachinesModule(InstancesRoutePrefix prefix,
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory, 
            IStateMachineFactory stateMachineFactory) 
            : base(prefix, "operator")
        {
            InstantiateMachine(configurationRepositoryContextFactory,
                instanceRepositoryContextFactory);

            GetMachineState(instanceRepositoryContextFactory);

            IsInState(configurationRepositoryContextFactory,
                instanceRepositoryContextFactory, stateMachineFactory);

            GetAvailableTriggers(configurationRepositoryContextFactory,
                instanceRepositoryContextFactory, stateMachineFactory);

            FireTrigger(configurationRepositoryContextFactory,
                instanceRepositoryContextFactory, stateMachineFactory);

            GetDiagramForInstance(configurationRepositoryContextFactory,
                instanceRepositoryContextFactory, stateMachineFactory);

            DeleteInstance(instanceRepositoryContextFactory);
        }

        private void GetDiagramForInstance(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["GetDiagramForInstance", "/{MachineInstanceId}/diagram", true] = async (parameters, ct) =>
            {
                string machineInstanceId = parameters.machineInstanceId;
                IStateMachineConfiguration configuration;

                using (var repository = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines.RetrieveMachineConfigurationForInstance(machineInstanceId, true, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        machineInstanceId,
                        configuration,
                        instanceRepositoryContextFactory);

                return Response.AsText(machine.ToString(), "text/plain");
            };

        private void DeleteInstance(IInstanceRepositoryContextFactory instanceRepositoryContextFactory) =>
            Delete["DeleteInstance", "/{MachineInstanceId}", true] = async (parameters, ct) =>
            {
                string machineInstanceId = parameters.machineInstanceId;

                using (var repository = instanceRepositoryContextFactory.OpenInstanceRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    await repository.MachineInstances.DeleteInstance(machineInstanceId, ct);
                }

                return 200;
            };

        private void FireTrigger(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Post["FireTrigger", "/{MachineInstanceId}/fire/{TriggerName}", true] = async (parameters, ct) =>
            {
                var triggerFireRequest = this.Bind<TriggerFireRequest>();
                IStateMachineConfiguration configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines
                        .RetrieveMachineConfigurationForInstance(triggerFireRequest.MachineInstanceId, true, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        triggerFireRequest.MachineInstanceId,
                        configuration,
                        instanceRepositoryContextFactory);

                try
                {
                    machine.Fire(new Trigger(configuration.MachineDefinition.MachineDefinitionId,
                        triggerFireRequest.TriggerName),
                        triggerFireRequest.Payload);
                }
                catch (InvalidOperationException ex)
                {
                    return Negotiate
                        .WithStatusCode(400)
                        .WithReasonPhrase(ex.Message)
                        .WithModel(new {reasonPhrase = ex.Message})
                        .WithAllowedMediaRange(new MediaRange("application/json"));
                }
                catch (StateConflictException ex)
                {
                    return Negotiate
                        .WithStatusCode(409)
                        .WithReasonPhrase(ex.Message)
                        .WithModel(new { reasonPhrase = ex.Message })
                        .WithAllowedMediaRange(new MediaRange("application/json"));
                }
                catch (AggregateException ex) 
                    when(ex.InnerExceptions.First().GetType() == typeof(StateConflictException))
                {
                    return Negotiate
                        .WithStatusCode(409)
                        .WithReasonPhrase(ex.InnerExceptions.First().Message)
                        .WithModel(new { reasonPhrase = ex.InnerExceptions.First().Message })
                        .WithAllowedMediaRange(new MediaRange("application/json"));
                }

                State currentState;
                using (var repository = instanceRepositoryContextFactory
                    .OpenInstanceRepositoryContext(Context.CurrentUser.GetApiKey()))
                        currentState = repository.MachineInstances.GetInstanceState(triggerFireRequest.MachineInstanceId);

                return Negotiate
                    .WithModel(currentState)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void GetAvailableTriggers(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["GetAvailableTriggers", "/{MachineInstanceId}/triggers", true] = async (parameters, ct) =>
            {
                string machineInstanceId = parameters.MachineInstanceId;
                IStateMachineConfiguration configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines.RetrieveMachineConfigurationForInstance(machineInstanceId, true, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        machineInstanceId,
                        configuration,
                        instanceRepositoryContextFactory);

                return Negotiate
                    .WithModel(machine.PermittedTriggers)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void IsInState(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["IsInState", "/{MachineInstanceId}/isinstate/{StateName}", true] = async (parameters, ct) =>
            {
                string machineInstanceId = parameters.MachineInstanceId;
                string isInStateName = parameters.StateName;
                bool isInState;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    var configuration = await repository.Machines
                        .RetrieveMachineConfiguration(machineInstanceId, false, ct);

                    var machine = stateMachineFactory
                        .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                            machineInstanceId,
                            configuration,
                            instanceRepositoryContextFactory);

                    isInState = machine.IsInState(
                        new State(configuration.MachineDefinition.MachineDefinitionId, isInStateName));
                }

                return Negotiate
                    .WithModel(new { queriedState = isInStateName, isInState })
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void GetMachineState(IInstanceRepositoryContextFactory instanceRepositoryContextFactory) =>
            Get["GetMachineState", "/{MachineInstanceId}/state"] = (parameters) =>
            {
                string machineInstanceId = parameters.MachineInstanceId;
                State currentState;

                using (var repository = instanceRepositoryContextFactory.OpenInstanceRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    currentState = repository.MachineInstances.GetInstanceState(machineInstanceId);
                }

                if (currentState == null)
                    return Negotiate
                        .WithStatusCode(400)
                        .WithReasonPhrase("The machine instance requested does not exist.")
                        .WithAllowedMediaRange(new MediaRange("application/json"));

                return Negotiate
                    .WithModel(currentState)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void InstantiateMachine(
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory) =>
            Post["InstantiateMachine", "/instantiate/{MachineDefinitionId}", true] = async (parameters, ct) =>
            {
                string machineDefinitionId = parameters.MachineDefinitionId;
                string machineInstanceId = Guid.NewGuid().ToString();

                using (var configruationRepository = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                using (var instanceRepository = instanceRepositoryContextFactory.OpenInstanceRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    var machineConfiguration = await configruationRepository.Machines.RetrieveMachineConfiguration(machineDefinitionId, false, ct);

                    await instanceRepository.MachineInstances.EnsureInstanceExists(machineConfiguration, machineInstanceId, ct);
                }

                return Negotiate
                    .WithModel(new { machineInstanceId })
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
    }
}