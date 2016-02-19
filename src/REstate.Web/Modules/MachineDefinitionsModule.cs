using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using REstate.Configuration;
using REstate.Repositories;
using REstate.Services;

namespace REstate.Web.Modules
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
        /// <param name="repositoryContextFactory">The repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        public MachineDefinitionsModule(IRepositoryContextFactory repositoryContextFactory,
            IStateMachineFactory stateMachineFactory)
            : base("/machinedefinitions", "machineBuilder")
        {
            GetMachineDefinition(repositoryContextFactory);

            GetDiagramForDefinition(repositoryContextFactory, stateMachineFactory);

            DefineMachine(repositoryContextFactory);

            DefineStates(repositoryContextFactory);

            DefineTriggers(repositoryContextFactory);

            DefineTransitions(repositoryContextFactory);

            DefineIgnoreRules(repositoryContextFactory);

            SetInitialState(repositoryContextFactory);

            DefineGuards(repositoryContextFactory);

            UpdateTransition(repositoryContextFactory);

            UpdateGuard(repositoryContextFactory);

            ToggleMachineDefinitionActive(repositoryContextFactory);

            UpdateMachineDefinition(repositoryContextFactory);
        }

        private void UpdateMachineDefinition(IRepositoryContextFactory repositoryContextFactory)
        {
            Put["UpdateMachineDefinition", "/", true] = async (parameters, ct) =>
            {
                IMachineDefinition machineDefinition = this.Bind<MachineDefinition>();

                IMachineDefinition newMachineDefinition;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newMachineDefinition = await repository.Configuration.UpdateMachineDefinition(machineDefinition, ct);
                }

                return Negotiate
                    .WithModel(newMachineDefinition)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void ToggleMachineDefinitionActive(IRepositoryContextFactory repositoryContextFactory)
        {
            Patch["ToggleMachineDefinitionActive", "/{MachineDefinitionId:int}/toggle/{IsActive:bool}", true] =
                async (parameters, ct) =>
                {
                    int machineDefinitionId = parameters.MachineDefintionId;
                    bool isActive = parameters.IsActive;

                    IMachineDefinition machineDefinition;

                    using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                    {
                        machineDefinition = await repository.Configuration.ToggleMachineDefinitionActive(machineDefinitionId, isActive, ct);
                    }

                    return Negotiate
                        .WithModel(machineDefinition)
                        .WithAllowedMediaRange(new MediaRange("application/json"));
                };
        }

        private void UpdateGuard(IRepositoryContextFactory repositoryContextFactory)
        {
            Put["UpdateGuard", "/guards/", true] = async (parameters, ct) =>
            {
                IGuard guard = this.Bind<Guard>();

                IGuard newGuard;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newGuard = await repository.Configuration.UpdateGuard(guard, ct);
                }

                return Negotiate
                    .WithModel(newGuard)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void UpdateTransition(IRepositoryContextFactory repositoryContextFactory)
        {
            Put["UpdateTransition", "/transitions/", true] = async (parameters, ct) =>
            {
                ITransition transition = this.Bind<Transition>();

                ITransition newTransition;
                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newTransition = await repository.Configuration.UpdateTransition(transition, ct);
                }

                return Negotiate
                    .WithModel(newTransition)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineGuards(IRepositoryContextFactory repositoryContextFactory)
        {
            Post["DefineGuards", "/guards/", true] = async (parameters, ct) =>
            {
                ICollection<IGuard> requestedGuards = this.Bind<List<Guard>>()
                    .Cast<IGuard>()
                    .ToList();

                ICollection<IGuard> newGuards;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newGuards = await repository.Configuration.DefineGuards(requestedGuards, ct);
                }

                return Negotiate
                    .WithModel(newGuards)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void SetInitialState(IRepositoryContextFactory repositoryContextFactory)
        {
            Patch["SetInitialState", "/{MachineDefinitionId:int}/initialstate/{InitialStateName}", true] = async (parameters, ct) =>
            {
                int machineDefinitionId = parameters.MachineDefinitionId;
                string initialStateName = parameters.InitialStateName;
                IMachineDefinition machineDefinition;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    machineDefinition = await repository.Configuration.SetInitialState(machineDefinitionId, initialStateName, ct);
                }

                return Negotiate
                    .WithModel(machineDefinition)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineIgnoreRules(IRepositoryContextFactory repositoryContextFactory)
        {
            Post["DefineIgnoreRules", "/ignorerules", true] = async (parameters, ct) =>
            {
                var requestedIgnoreRules = this.Bind<IgnoreRule[]>();
                ICollection<IIgnoreRule> newIgnoreRules;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newIgnoreRules = await repository.Configuration.DefineIgnoreRules(requestedIgnoreRules, ct);
                }

                return Negotiate
                    .WithModel(newIgnoreRules)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineTransitions(IRepositoryContextFactory repositoryContextFactory)
        {
            Post["DefineTransitions", "/{MachineDefinitionId:int}/transitions", true] = async (parameters, ct) =>
            {
                var requestedTransitions = this.Bind<Transition[]>();
                ICollection<ITransition> newTransitions;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newTransitions = await repository.Configuration.DefineTransitions(requestedTransitions, ct);
                }

                return Negotiate
                    .WithModel(newTransitions)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineTriggers(IRepositoryContextFactory repositoryContextFactory)
        {
            Post["DefineTriggers", "/{MachineDefinitionId:int}/triggers", true] = async (parameters, ct) =>
            {
                var requestedTriggers = this.Bind<Configuration.Trigger[]>();
                ICollection<ITrigger> newTriggers;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newTriggers = await repository.Configuration.DefineTriggers(requestedTriggers, ct);
                }

                return Negotiate
                    .WithModel(newTriggers)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineMachine(IRepositoryContextFactory repositoryContextFactory)
        {
            Post["DefineMachine", "/", true] = async (parameters, ct) =>
            {
                IMachineDefinition definiton = this.Bind<MachineDefinition>(BindingConfig.Default,
                    "MachineDefinitionId", "InitialStateName", "IsActive");

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    definiton = await repository.Configuration
                        .DefineMachine(definiton, ct);
                }

                return Negotiate
                    .WithModel(definiton)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void DefineStates(IRepositoryContextFactory repositoryContextFactory)
        {
            Post["DefineStates", "/{MachineDefinitionId:int}/states", true] = async (parameters, ct) =>
            {
                ICollection<IState> requestedStates = this.Bind<List<Configuration.State>>()
                    .Cast<IState>()
                    .ToList();

                ICollection<IState> newStates;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newStates = await repository.Configuration.DefineStates(requestedStates, ct);
                }

                return Negotiate
                    .WithModel(newStates)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };
        }

        private void GetMachineDefinition(IRepositoryContextFactory repositoryContextFactory) =>
            Get["GetMachineDefinition", "/{MachineDefinitionId:int}", true] = async (parameters, ct) =>
            {
                int machineDefinitionId = parameters.MachineDefinitionId;
                IStateMachineConfiguration configuration;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Configuration
                        .RetrieveMachineConfiguration(machineDefinitionId, ct);
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

        private void GetDiagramForDefinition(IRepositoryContextFactory repositoryContextFactory, IStateMachineFactory stateMachineFactory) =>
            Get["GetDiagramForDefinition", "/{MachineDefinitionId:int}/diagram", true] = async (parameters, ct) =>
            {
                int machineDefinitionId = parameters.MachineDefinitionId;
                IStateMachineConfiguration configuration;

                using (var repository = repositoryContextFactory.OpenRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Configuration.RetrieveMachineConfiguration(machineDefinitionId, ct);
                }

                var machine = stateMachineFactory.ConstructFromConfiguration(Context.CurrentUser.GetApiKey(), configuration);

                return Response.AsText(machine.ToString(), "text/plain");
            };

    }
}