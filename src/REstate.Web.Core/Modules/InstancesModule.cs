using System;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Psibr.Platform;
using Psibr.Platform.Logging;
using Psibr.Platform.Nancy.Modules;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using REstate.Services;
using REstate.Web.Core.Requests;
using REstate.Web.Core.Responses;

namespace REstate.Web.Core.Modules
{
    /// <summary>
    /// Machine interactions module.
    /// </summary>
    public class InstancesModule
        : SecuredModule
    {
        protected IPlatformLogger Logger { get; set; }

        /// <summary>
        /// Registers routes for interacting with machines.
        /// </summary>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        /// <param name="logger"></param>
        public InstancesModule(
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory,
            IPlatformLogger logger)
            : base("/instances", claim => claim.Type == "claim" && claim.Value == "operator")
        {
            Logger = logger;

            GetMachineState(configurationRepositoryContextFactory);

            IsInState(configurationRepositoryContextFactory,
                stateMachineFactory);

            GetAvailableTriggers(configurationRepositoryContextFactory,
                stateMachineFactory);

            FireTrigger(configurationRepositoryContextFactory,
                stateMachineFactory);

            GetDiagramForInstance(configurationRepositoryContextFactory,
                stateMachineFactory);

            GetInstanceInfo(configurationRepositoryContextFactory);

            DeleteInstance(configurationRepositoryContextFactory);
        }

        private void GetInstanceInfo(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Get("/{MachineInstanceId}", async (parameters, ct) =>
            {
                string machineInstanceId = parameters.machineInstanceId;
                string metadata;


                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    metadata = await repository.MachineInstances.GetInstanceMetadata(machineInstanceId, ct);
                }

                return Response.AsText(metadata ?? "{ }", "application/json");
            });

        private void GetDiagramForInstance(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get("/{MachineInstanceId}/diagram", async (parameters, ct) =>
            {
                string machineInstanceId = parameters.machineInstanceId;
                Machine configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines.RetrieveMachineConfigurationForInstance(machineInstanceId, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        machineInstanceId,
                        configuration,
                        configurationRepositoryContextFactory);

                return Response.AsText(machine.ToString(), "text/plain");
            });

        private void DeleteInstance(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Delete("/{MachineInstanceId}", async (parameters, ct) =>
            {
                string machineInstanceId = parameters.machineInstanceId;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    await repository.MachineInstances.DeleteInstance(machineInstanceId, ct);
                }

                return HttpStatusCode.Accepted;
            });

        private void FireTrigger(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Post("/{MachineInstanceId}/fire/{TriggerName}", async (parameters, ct) =>
            {
                var triggerFireRequest = this.Bind<TriggerFireRequest>();
                InstanceRecord instanceRecord;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    var configuration = await repository.Machines
                        .RetrieveMachineConfigurationForInstance(triggerFireRequest.MachineInstanceId, ct);


                    var machine = stateMachineFactory
                        .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                            triggerFireRequest.MachineInstanceId,
                            configuration,
                            configurationRepositoryContextFactory);

                    try
                    {
                        machine.Fire(new Trigger(configuration.MachineName,
                            triggerFireRequest.TriggerName),
                            triggerFireRequest.Payload);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Negotiate
                            .WithStatusCode(400)
                            .WithReasonPhrase(ex.Message)
                            .WithModel(new ReasonPhraseResponse { ReasonPhrase = ex.Message});
                    }
                    catch (StateConflictException ex)
                    {
                        return Negotiate
                            .WithStatusCode(409)
                            .WithReasonPhrase(ex.Message)
                            .WithModel(new ReasonPhraseResponse {ReasonPhrase = ex.Message});
                    }
                    catch (AggregateException ex)
                        when (ex.InnerExceptions.First().GetType() == typeof(StateConflictException))
                    {
                        return Negotiate
                            .WithStatusCode(409)
                            .WithReasonPhrase(ex.InnerExceptions.First().Message)
                            .WithModel(new ReasonPhraseResponse { ReasonPhrase = ex.InnerExceptions.First().Message });
                    }


                    instanceRecord = repository.MachineInstances.GetInstanceState(triggerFireRequest.MachineInstanceId);
                }

                return instanceRecord;
            });

        private void GetAvailableTriggers(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get("/{MachineInstanceId}/triggers", async (parameters, ct) =>
            {
                string machineInstanceId = parameters.MachineInstanceId;
                Machine configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines
                        .RetrieveMachineConfigurationForInstance(machineInstanceId, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        machineInstanceId,
                        configuration,
                        configurationRepositoryContextFactory);

                return machine.PermittedTriggers.Select(trigger =>
                    new Responses.Trigger
                    {
                        MachineName = trigger.MachineDefinitionId,
                        TriggerName = trigger.TriggerName
                    }).ToList();
            });

        private void IsInState(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get("/{MachineInstanceId}/isinstate/{StateName}", async (parameters, ct) =>
            {
                string machineInstanceId = parameters.MachineInstanceId;
                string isInStateName = parameters.StateName;
                bool isInState;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    var configuration = await repository.Machines
                        .RetrieveMachineConfigurationForInstance(machineInstanceId, ct);

                    var machine = stateMachineFactory
                        .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                            machineInstanceId,
                            configuration,
                            configurationRepositoryContextFactory);

                    isInState = machine.IsInState(
                        new State(configuration.MachineName, isInStateName));
                }

                return new IsInStateResponse { QueriedStateName = isInStateName, IsInState = isInState };
            });

        private void GetMachineState(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Get("/{MachineInstanceId}/state", (parameters) =>
            {
                string machineInstanceId = parameters.MachineInstanceId;
                InstanceRecord instanceRecord;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    instanceRecord = repository.MachineInstances.GetInstanceState(machineInstanceId);

                    if (instanceRecord == null)
                        return new Response
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            ReasonPhrase = "The machine instance requested does not exist."
                        };
                }

                return instanceRecord;
            });
    }
}