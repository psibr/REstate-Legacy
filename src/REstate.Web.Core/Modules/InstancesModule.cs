using System;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Modules;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using REstate.Services;
using REstate.Web.Core.Requests;

namespace REstate.Web.Core.Modules
{
    /// <summary>
    /// Machine interactions module.
    /// </summary>
    public class InstancesModule
        : SecuredModule
    {
        /// <summary>
        /// Registers routes for interacting with machines.
        /// </summary>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        public InstancesModule(
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory)
            : base("/instances", claim => claim.Type == "claim" && claim.Value == "operator")
        {

            GetMachineState(configurationRepositoryContextFactory);

            IsInState(configurationRepositoryContextFactory,
                stateMachineFactory);

            GetAvailableTriggers(configurationRepositoryContextFactory,
                stateMachineFactory);

            FireTrigger(configurationRepositoryContextFactory,
                stateMachineFactory);

            GetDiagramForInstance(configurationRepositoryContextFactory,
                stateMachineFactory);

            DeleteInstance(configurationRepositoryContextFactory);
        }

        private void GetDiagramForInstance(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["GetDiagramForInstance", "/{MachineInstanceId}/diagram"] = async (parameters, ct) =>
            {
                string machineInstanceId = parameters.machineInstanceId;
                Machine configuration;

                using (var repository = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines.RetrieveMachineConfigurationForInstance(machineInstanceId, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        machineInstanceId,
                        configuration,
                        configurationRepositoryContextFactory);

                return Response.AsText(machine.ToString(), "text/plain");
            };

        private void DeleteInstance(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Delete["DeleteInstance", "/{MachineInstanceId}"] = async (parameters, ct) =>
            {
                string machineInstanceId = parameters.machineInstanceId;

                using (var repository = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    await repository.MachineInstances.DeleteInstance(machineInstanceId, ct);
                }

                return 200;
            };

        private void FireTrigger(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Post["FireTrigger", "/{MachineInstanceId}/fire/{TriggerName}"] = async (parameters, ct) =>
            {
                var triggerFireRequest = this.Bind<TriggerFireRequest>();
                State currentState;

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
                            .WithModel(new { reasonPhrase = ex.Message })
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
                        when (ex.InnerExceptions.First().GetType() == typeof(StateConflictException))
                    {
                        return Negotiate
                            .WithStatusCode(409)
                            .WithReasonPhrase(ex.InnerExceptions.First().Message)
                            .WithModel(new { reasonPhrase = ex.InnerExceptions.First().Message })
                            .WithAllowedMediaRange(new MediaRange("application/json"));
                    }


                    var instanceRecord = repository.MachineInstances.GetInstanceState(triggerFireRequest.MachineInstanceId);

                    currentState = new State(instanceRecord.MachineName, instanceRecord.StateName);
                }

                return Negotiate
                    .WithModel(currentState)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void GetAvailableTriggers(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["GetAvailableTriggers", "/{MachineInstanceId}/triggers"] = async (parameters, ct) =>
            {
                string machineInstanceId = parameters.MachineInstanceId;
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

                return Negotiate
                    .WithModel(machine.PermittedTriggers)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void IsInState(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["IsInState", "/{MachineInstanceId}/isinstate/{StateName}"] = async (parameters, ct) =>
            {
                string machineInstanceId = parameters.MachineInstanceId;
                string isInStateName = parameters.StateName;
                bool isInState;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    Machine configuration = await repository.Machines
                        .RetrieveMachineConfigurationForInstance(machineInstanceId, ct);

                    var machine = stateMachineFactory
                        .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                            machineInstanceId,
                            configuration,
                            configurationRepositoryContextFactory);

                    isInState = machine.IsInState(
                        new State(configuration.MachineName, isInStateName));
                }

                return Negotiate
                    .WithModel(new { queriedState = isInStateName, isInState })
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void GetMachineState(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Get["GetMachineState", "/{MachineInstanceId}/state"] = (parameters, ct) =>
            {
                string machineInstanceId = parameters.MachineInstanceId;
                State currentState;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    var instanceRecord = repository.MachineInstances.GetInstanceState(machineInstanceId);

                    if (instanceRecord == null)
                        return Task.FromResult<dynamic>(Negotiate
                            .WithStatusCode(400)
                            .WithReasonPhrase("The machine instance requested does not exist.")
                            .WithAllowedMediaRange(new MediaRange("application/json")));

                    currentState = new State(instanceRecord.MachineName, instanceRecord.StateName);
                }

                return Task.FromResult<dynamic>(Negotiate
                    .WithModel(currentState)
                    .WithAllowedMediaRange(new MediaRange("application/json")));
            };
    }
}