using System;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using REstate.Configuration;
using REstate.Repositories;
using REstate.Services;
using REstate.Web.Requests;

namespace REstate.Web.Modules
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
        /// <param name="repositoryContextFactory">The repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        public MachinesModule(IRepositoryContextFactory repositoryContextFactory, IStateMachineFactory stateMachineFactory)
            : base("/machines")
        {
            InstantiateMachine(repositoryContextFactory);

            GetMachineState(repositoryContextFactory);

            IsInState(repositoryContextFactory, stateMachineFactory);

            GetAvailableTriggers(repositoryContextFactory, stateMachineFactory);

            FireTrigger(repositoryContextFactory, stateMachineFactory);

            GetDiagramForInstance(repositoryContextFactory, stateMachineFactory);

            DeleteInstance(repositoryContextFactory);
        }

        private void GetDiagramForInstance(IRepositoryContextFactory repositoryContextFactory, IStateMachineFactory stateMachineFactory) =>
            Get["GetDiagramForInstance", "/{MachineInstanceGuid:guid}/diagram", true] = async (parameters, ct) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;
                IStateMachineConfiguration configuration;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Configuration.RetrieveMachineConfiguration(machineInstanceGuid, ct);
                }

                var machine = stateMachineFactory.ConstructFromConfiguration(Context.CurrentUser.GetApiKey(), machineInstanceGuid, configuration);

                return Response.AsText(machine.ToString(), "text/plain");
            };

        private void DeleteInstance(IRepositoryContextFactory repositoryContextFactory) =>
            Delete["DeleteInstance", "/{MachineInstanceGuid:guid}", true] = async (parameters, ct) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    await repository.MachineInstances.DeleteInstance(machineInstanceGuid, ct);
                }

                return 200;
            };

        private void FireTrigger(IRepositoryContextFactory repositoryContextFactory, IStateMachineFactory stateMachineFactory) =>
            Post["FireTrigger", "/{MachineInstanceGuid:guid}/fire/{TriggerName}", true] = async (parameters, ct) =>
            {
                var triggerFireRequest = this.Bind<TriggerFireRequest>();
                IStateMachineConfiguration configuration;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Configuration.RetrieveMachineConfiguration(triggerFireRequest.MachineInstanceGuid, ct);
                }

                var machine = stateMachineFactory.ConstructFromConfiguration(Context.CurrentUser.GetApiKey(), triggerFireRequest.MachineInstanceGuid, configuration);

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
                        .WithModel(new { reasonPhrase = ex.Message })
                        .WithAllowedMediaRange(new MediaRange("application/json"));
                }

                var currentState = machine.GetCurrentState();

                return Negotiate
                    .WithModel(currentState)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void GetAvailableTriggers(IRepositoryContextFactory repositoryContextFactory, IStateMachineFactory stateMachineFactory) =>
            Get["GetAvailableTriggers", "/{MachineInstanceGuid:guid}/triggers", true] = async (parameters, ct) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;
                IStateMachineConfiguration configuration;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Configuration.RetrieveMachineConfiguration(machineInstanceGuid, ct);
                }

                var machine = stateMachineFactory.ConstructFromConfiguration(Context.CurrentUser.GetApiKey(), machineInstanceGuid, configuration);

                return Negotiate
                    .WithModel(machine.PermittedTriggers)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void IsInState(IRepositoryContextFactory repositoryContextFactory, IStateMachineFactory stateMachineFactory) =>
            Get["IsInState", "/{MachineInstanceGuid:guid}/isinstate/{StateName}", true] = async (parameters, ct) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;
                string isInStateName = parameters.StateName;
                bool isInState;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    var configuration = await repository.Configuration.RetrieveMachineConfiguration(machineInstanceGuid, ct);
                    var machine = stateMachineFactory.ConstructFromConfiguration(Context.CurrentUser.GetApiKey(), machineInstanceGuid, configuration);

                    isInState = machine.IsInState(new State(configuration.MachineDefinition.MachineDefinitionId, isInStateName));
                }

                return Negotiate
                    .WithModel(new { queriedState = isInStateName, isInState })
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void GetMachineState(IRepositoryContextFactory repositoryContextFactory) =>
            Get["GetMachineState", "/{MachineInstanceGuid:guid}/state", true] = async (parameters, ct) =>
            {
                Guid machineInstanceGuid = parameters.MachineInstanceGuid;
                State currentState;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    currentState = await repository.MachineInstances.GetInstanceState(machineInstanceGuid, ct);
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

        private void InstantiateMachine(IRepositoryContextFactory repositoryContextFactory) =>
            Post["InstantiateMachine", "/instantiate/{MachineDefinitionId:int}", true] = async (parameters, ct) =>
            {
                int machineDefinitionId = parameters.MachineDefinitionId;
                var machineInstanceGuid = Guid.NewGuid();

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    var machineConfiguration = await repository.Configuration.RetrieveMachineConfiguration(machineDefinitionId, ct);

                    await repository.MachineInstances.EnsureInstanceExists(machineConfiguration, machineInstanceGuid, ct);
                }

                return Negotiate
                    .WithModel(new { machineInstanceGuid })
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
    }
}