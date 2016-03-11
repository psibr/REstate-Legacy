using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using REstate.Services;
using REstate.Web.Configuration.Requests;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Web.Configuration.Modules
{
    /// <summary>
    /// Machine Definitions configuration module.
    /// </summary>
    public class MachineDefinitionsModule
        : ConfigurationModule
    {
        /// <summary>
        /// Registers routes for defining new machines.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        public MachineDefinitionsModule(ConfigurationRoutePrefix prefix,
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory)
            : base(prefix + "/machinedefinitions", "machineBuilder")
        {
            GetMachine(configurationRepositoryContextFactory);

            GetDiagramForDefinition(configurationRepositoryContextFactory, stateMachineFactory);

            DefineMachine(configurationRepositoryContextFactory);

            DefineStates(configurationRepositoryContextFactory);

            DefineTriggers(configurationRepositoryContextFactory);

            DefineTransitions(configurationRepositoryContextFactory);

            DefineIgnoreRules(configurationRepositoryContextFactory);

            SetInitialState(configurationRepositoryContextFactory);

            DefineGuards(configurationRepositoryContextFactory);

            UpdateTransition(configurationRepositoryContextFactory);

            UpdateGuard(configurationRepositoryContextFactory);

            ToggleMachineDefinitionActive(configurationRepositoryContextFactory);

            UpdateMachineDefinition(configurationRepositoryContextFactory);

            DefineStateActions(configurationRepositoryContextFactory);

            DefineStateMachine(configurationRepositoryContextFactory);
        }

        private void DefineStateMachine(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineStateMachine", "/", true] = async (parameters, ct) =>
            {
                StateMachineConfiguration stateMachineConfiguration = this.Bind<StateMachineConfigurationRequest>();
                IStateMachineConfiguration newMachineConfiguration;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newMachineConfiguration = await repository.Machines.DefineStateMachine(stateMachineConfiguration, ct);
                }

                return Negotiate
                    .WithModel(newMachineConfiguration)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineStateActions(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineStateActions", "/stateactions/", true] = async (parameters, ct) =>
            {
                ICollection<IStateAction> requestedStateActions = this.Bind<List<StateAction>>()
                    .Cast<IStateAction>()
                    .ToList();

                ICollection<IStateAction> newStateActions;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newStateActions = await repository.Machines.DefineStateActions(requestedStateActions, ct);
                }

                return Negotiate
                    .WithModel(newStateActions)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void UpdateMachineDefinition(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Put["UpdateMachineDefinition", "/definition", true] = async (parameters, ct) =>
            {
                IMachineDefinition machineDefinition = this.Bind<MachineDefinition>();

                IMachineDefinition newMachineDefinition;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newMachineDefinition = await repository.Machines.UpdateMachineDefinition(machineDefinition, ct);
                }

                return Negotiate
                    .WithModel(newMachineDefinition)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void ToggleMachineDefinitionActive(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Patch["ToggleMachineDefinitionActive", "/{MachineDefinitionId:int}/toggle/{IsActive:bool}", true] =
                async (parameters, ct) =>
                {
                    int machineDefinitionId = parameters.MachineDefinitionId;
                    bool isActive = parameters.IsActive;

                    IMachineDefinition machineDefinition;

                    using (var repository = configurationRepositoryContextFactory
                        .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                    {
                        machineDefinition = await repository.Machines.ToggleMachineDefinitionActive(machineDefinitionId, isActive, ct);
                    }

                    return Negotiate
                        .WithModel(machineDefinition)
                        .WithAllowedMediaRange(new MediaRange("application/json"));
                };
        }

        private void UpdateGuard(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Put["UpdateGuard", "/guards/", true] = async (parameters, ct) =>
            {
                IGuard guard = this.Bind<Guard>();

                IGuard newGuard;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newGuard = await repository.Machines.UpdateGuard(guard, ct);
                }

                return Negotiate
                    .WithModel(newGuard)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void UpdateTransition(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Put["UpdateTransition", "/transitions/", true] = async (parameters, ct) =>
            {
                ITransition transition = this.Bind<Transition>();

                ITransition newTransition;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newTransition = await repository.Machines.UpdateTransition(transition, ct);
                }

                return Negotiate
                    .WithModel(newTransition)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineGuards(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineGuards", "/guards/", true] = async (parameters, ct) =>
            {
                ICollection<IGuard> requestedGuards = this.Bind<List<Guard>>()
                    .Cast<IGuard>()
                    .ToList();

                ICollection<IGuard> newGuards;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newGuards = await repository.Machines.DefineGuards(requestedGuards, ct);
                }

                return Negotiate
                    .WithModel(newGuards)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void SetInitialState(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Patch["SetInitialState", "/{MachineDefinitionId:int}/initialstate/{InitialStateName}", true] = async (parameters, ct) =>
            {
                int machineDefinitionId = parameters.MachineDefinitionId;
                string initialStateName = parameters.InitialStateName;
                IMachineDefinition machineDefinition;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    machineDefinition = await repository.Machines.SetInitialState(machineDefinitionId, initialStateName, ct);
                }

                return Negotiate
                    .WithModel(machineDefinition)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineIgnoreRules(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineIgnoreRules", "/ignorerules", true] = async (parameters, ct) =>
            {
                var requestedIgnoreRules = this.Bind<IgnoreRule[]>();
                ICollection<IIgnoreRule> newIgnoreRules;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newIgnoreRules = await repository.Machines.DefineIgnoreRules(requestedIgnoreRules, ct);
                }

                return Negotiate
                    .WithModel(newIgnoreRules)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineTransitions(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineTransitions", "/{MachineDefinitionId:int}/transitions", true] = async (parameters, ct) =>
            {
                var requestedTransitions = this.Bind<Transition[]>();
                ICollection<ITransition> newTransitions;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newTransitions = await repository.Machines.DefineTransitions(requestedTransitions, ct);
                }

                return Negotiate
                    .WithModel(newTransitions)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineTriggers(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineTriggers", "/{MachineDefinitionId:int}/triggers", true] = async (parameters, ct) =>
            {
                var requestedTriggers = this.Bind<REstate.Configuration.Trigger[]>();
                ICollection<ITrigger> newTriggers;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newTriggers = await repository.Machines.DefineTriggers(requestedTriggers, ct);
                }

                return Negotiate
                    .WithModel(newTriggers)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineMachine(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineMachine", "/definition", true] = async (parameters, ct) =>
            {
                IMachineDefinition definiton = this.Bind<MachineDefinition>(BindingConfig.Default,
                    "MachineDefinitionId", "InitialStateName", "IsActive");

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    definiton = await repository.Machines
                        .DefineMachine(definiton, ct);
                }

                return Negotiate
                    .WithModel(definiton)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineStates(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineStates", "/{MachineDefinitionId:int}/states", true] = async (parameters, ct) =>
            {
                ICollection<IState> requestedStates = this.Bind<List<REstate.Configuration.State>>()
                    .Cast<IState>()
                    .ToList();

                ICollection<IState> newStates;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newStates = await repository.Machines.DefineStates(requestedStates, ct);
                }

                return Negotiate
                    .WithModel(newStates)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetMachine(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Get["GetMachine", "/{MachineDefinitionId:int}", true] = async (parameters, ct) =>
            {
                int machineDefinitionId = parameters.MachineDefinitionId;
                IStateMachineConfiguration configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines
                        .RetrieveMachineConfiguration(machineDefinitionId, false, ct);
                }

                if (configuration == null)
                    return new NotFoundResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        ReasonPhrase = "The definition was not found.",
                        ContentType = "application/json",
                        Contents = stream => stream.Close()
                    };

                return Negotiate
                    .WithModel(configuration)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void GetDiagramForDefinition(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["GetDiagramForDefinition", "/{MachineDefinitionId:int}/diagram", true] = async (parameters, ct) =>
            {
                int machineDefinitionId = parameters.MachineDefinitionId;
                IStateMachineConfiguration configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines
                        .RetrieveMachineConfiguration(machineDefinitionId, true, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(), configuration);

                return Response.AsText(machine.ToString(), "text/plain");
            };

    }
}