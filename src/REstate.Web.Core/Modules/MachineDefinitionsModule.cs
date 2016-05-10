using System;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
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
        /// <summary>
        /// Registers routes for defining new machines.
        /// </summary>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        public MachineDefinitionsModule(
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory)
            : base("/machines", "machineBuilder")
        {
            GetMachine(configurationRepositoryContextFactory);

            GetDiagramForDefinition(configurationRepositoryContextFactory, stateMachineFactory);

            DefineStateMachine(configurationRepositoryContextFactory);

            ListMachines(configurationRepositoryContextFactory);

            InstantiateMachine(configurationRepositoryContextFactory);

            PreviewDiagram(stateMachineFactory);
        }


        private void InstantiateMachine(
            IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Post["InstantiateMachine", "{MachineDefinitionId}/instantiate/", true] = async (parameters, ct) =>
            {
                string machineDefinitionId = parameters.MachineDefinitionId;
                string machineInstanceId = Guid.NewGuid().ToString();

                using (var configruationRepository = configurationRepositoryContextFactory.OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    await configruationRepository.MachineInstances.EnsureInstanceExists(machineDefinitionId, machineInstanceId, ct);
                }

                return Negotiate
                    .WithModel(new { machineInstanceId })
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void ListMachines(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Get["ListDefinitions", "/", true] = async (parameters, ct) =>
            {
                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    return Negotiate
                        .WithModel(await repository.Machines.ListMachines(ct))
                        .WithAllowedMediaRange(new MediaRange("application/json"));
                }
            };
        }

        private void DefineStateMachine(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory)
        {
            Post["DefineStateMachine", "/", true] = async (parameters, ct) =>
            {
                var stateMachineConfiguration = this.Bind<Machine>();
                Machine newMachineConfiguration;
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

        private void PreviewDiagram(IStateMachineFactory stateMachineFactory)
        {
            Post["PreviewDiagram", "/preview", true] = async (parameters, ct) =>
            {
                try
                {
                    var stateMachineConfiguration = this.Bind<Machine>();

                    var machine = stateMachineFactory.ConstructFromConfiguration(Context.CurrentUser.GetApiKey(),
                        stateMachineConfiguration);

                    return await Task.FromResult<dynamic>(Response.AsText(machine.ToString(), "text/plain"));
                }
                catch (Exception ex)
                {
                    return Negotiate.WithStatusCode(403)
                        .WithReasonPhrase(ex.Message);
                }

            };
        }

        private void GetMachine(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Get["GetMachine", "/{MachineDefinitionId}", true] = async (parameters, ct) =>
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
            };

        private void GetDiagramForDefinition(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["GetDiagramForDefinition", "/{MachineDefinitionId}/diagram", true] = async (parameters, ct) =>
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
            };

    }
}