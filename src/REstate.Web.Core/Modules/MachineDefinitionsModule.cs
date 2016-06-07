using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Psibr.Platform.Logging;
using Psibr.Platform.Nancy;
using Psibr.Platform.Nancy.Modules;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using REstate.Services;

namespace REstate.Web.Core.Modules
{
    /// <summary>
    /// Machine Definitions configuration module.
    /// </summary>
    public class MachineDefinitionsModule
        : SecuredModule
    {
        protected IPlatformLogger Logger { get; set; }

        /// <summary>
        /// Registers routes for defining new machines.
        /// </summary>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        /// <param name="logger"></param>
        public MachineDefinitionsModule(
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory,
            IPlatformLogger logger)
            : base("/machines", claim => claim.Type == "claim" && claim.Value == "machineBuilder")
        {
            Logger = logger;

            GetMachine(configurationRepositoryContextFactory);

            GetDiagramForDefinition(configurationRepositoryContextFactory, stateMachineFactory);

            DefineStateMachine(configurationRepositoryContextFactory);

            ListMachines(configurationRepositoryContextFactory);

            InstantiateMachine(configurationRepositoryContextFactory);

            PreviewDiagram(stateMachineFactory);
        }


        private void InstantiateMachine(
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Post("{MachineDefinitionId}/instantiate/", async (parameters, ct) =>
            {
                var metadata = this.Bind<IDictionary<string, string>>();

                string machineDefinitionId = parameters.MachineDefinitionId;
                var machineInstanceId = Guid.NewGuid().ToString();

                using (var configruationRepository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    await configruationRepository.MachineInstances.CreateInstance(machineDefinitionId, machineInstanceId, metadata, ct);
                }

                return Negotiate
                    .WithModel(new { machineInstanceId })
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            });

        private void ListMachines(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Get("/", async (parameters, ct) =>
            {
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    return Negotiate
                        .WithModel(await repository.Machines.ListMachines(ct))
                        .WithAllowedMediaRange(new MediaRange("application/json"));
                }
            });
        }

        private void DefineStateMachine(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post("/", async (parameters, ct) =>
            {
                var stateMachineConfiguration = this.Bind<Machine>();
                Machine newMachineConfiguration;
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    newMachineConfiguration = await repository.Machines
                        .DefineStateMachine(stateMachineConfiguration, ct);
                }

                return Negotiate
                    .WithModel(newMachineConfiguration)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            });
        }

        private void PreviewDiagram(IStateMachineFactory stateMachineFactory)
        {
            Post("/preview", (parameters) =>
            {

                var stateMachineConfiguration = this.Bind<Machine>();

                var machine = stateMachineFactory.ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                    stateMachineConfiguration);

                if (machine == null)
                    throw new Exception("Unable to construct machine.");

                return Response.AsText(machine.ToString(), "text/plain");


            });
        }

        private void GetMachine(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Get("/{MachineDefinitionId}", async (parameters, ct) =>
            {
                string machineDefinitionId = parameters.MachineDefinitionId;
                Machine configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines
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
            });

        private void GetDiagramForDefinition(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get("/{MachineDefinitionId}/diagram", async (parameters, ct) =>
            {
                string machineDefinitionId = parameters.MachineDefinitionId;
                Machine configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines
                        .RetrieveMachineConfiguration(machineDefinitionId, ct);
                }

                var machine = stateMachineFactory
                    .ConstructFromConfiguration(Context.CurrentUser.GetApiKey(), configuration);

                return Response.AsText(machine.ToString(), "text/plain");
            });

    }
}