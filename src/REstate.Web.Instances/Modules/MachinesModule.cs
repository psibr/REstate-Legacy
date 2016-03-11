using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using REstate.Repositories.Instances;
using REstate.Services;
using REstate.Web.Instances.Requests;
using REstate.Web.Modules;
using System;
using System.Linq;

namespace REstate.Web.Instances.Modules
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
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        /// <param name="instanceRepositoryContextFactory">The instance repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        public MachinesModule(InstancesRoutePrefix prefix,
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory, 
            IStateMachineFactory stateMachineFactory) 
            : base(prefix)
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
            Get["GetDiagramForInstance", "/{MachineInstanceGuid:guid}/diagram", true] = async (parameters, ct) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;
                IStateMachineConfiguration configuration;

                using (var repository = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines.RetrieveMachineConfiguration(machineInstanceGuid, true, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        machineInstanceGuid,
                        configuration,
                        instanceRepositoryContextFactory);

                return Response.AsText(machine.ToString(), "text/plain");
            };

        private void DeleteInstance(IInstanceRepositoryContextFactory instanceRepositoryContextFactory) =>
            Delete["DeleteInstance", "/{MachineInstanceGuid:guid}", true] = async (parameters, ct) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;

                using (var repository = instanceRepositoryContextFactory.OpenInstanceRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    await repository.MachineInstances.DeleteInstance(machineInstanceGuid, ct);
                }

                return 200;
            };

        private void FireTrigger(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Post["FireTrigger", "/{MachineInstanceGuid:guid}/fire/{TriggerName}", true] = async (parameters, ct) =>
            {
                var triggerFireRequest = this.Bind<TriggerFireRequest>();
                IStateMachineConfiguration configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines
                        .RetrieveMachineConfiguration(triggerFireRequest.MachineInstanceGuid, true, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        triggerFireRequest.MachineInstanceGuid,
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
                        currentState = repository.MachineInstances.GetInstanceState(triggerFireRequest.MachineInstanceGuid);

                return Negotiate
                    .WithModel(currentState)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void GetAvailableTriggers(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["GetAvailableTriggers", "/{MachineInstanceGuid:guid}/triggers", true] = async (parameters, ct) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;
                IStateMachineConfiguration configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines.RetrieveMachineConfiguration(machineInstanceGuid, true, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        machineInstanceGuid,
                        configuration,
                        instanceRepositoryContextFactory);

                return Negotiate
                    .WithModel(machine.PermittedTriggers)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void IsInState(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IInstanceRepositoryContextFactory instanceRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["IsInState", "/{MachineInstanceGuid:guid}/isinstate/{StateName}", true] = async (parameters, ct) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;
                string isInStateName = parameters.StateName;
                bool isInState;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    var configuration = await repository.Machines
                        .RetrieveMachineConfiguration(machineInstanceGuid, false, ct);

                    var machine = stateMachineFactory
                        .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                            machineInstanceGuid,
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
            Get["GetMachineState", "/{MachineInstanceGuid:guid}/state"] = (parameters) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;
                State currentState;

                using (var repository = instanceRepositoryContextFactory.OpenInstanceRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    currentState = repository.MachineInstances.GetInstanceState(machineInstanceGuid);
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
            Post["InstantiateMachine", "/instantiate/{MachineDefinitionId:int}", true] = async (parameters, ct) =>
            {
                int machineDefinitionId = parameters.MachineDefinitionId;
                var machineInstanceGuid = Guid.NewGuid();

                using (var configruationRepository = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                using (var instanceRepository = instanceRepositoryContextFactory.OpenInstanceRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    var machineConfiguration = await configruationRepository.Machines.RetrieveMachineConfiguration(machineDefinitionId, false, ct);

                    await instanceRepository.MachineInstances.EnsureInstanceExists(machineConfiguration, machineInstanceGuid, ct);
                }

                return Negotiate
                    .WithModel(new { machineInstanceGuid })
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
    }
}