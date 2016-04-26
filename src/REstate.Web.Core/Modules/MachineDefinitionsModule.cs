using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using Psibr.Platform.Nancy;
using REstate.Configuration;
using REstate.Repositories.Configuration;
using REstate.Services;
using REstate.Web.Core.Requests;

namespace REstate.Web.Core.Modules
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

            DefineStateMachine(configurationRepositoryContextFactory);
            ListMachines(configurationRepositoryContextFactory);
        }

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

        private void GetMachine(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory) =>
            Get["GetMachine", "/{MachineDefinitionId}", true] = async (parameters, ct) =>
            {
                string machineDefinitionId = parameters.MachineDefinitionId;
                IStateMachineConfiguration configuration;

                using (var repository = configurationRepositoryContextFactory
                    .OpenConfigurationRepositoryContext(Context.CurrentUser.GetApiKey()))
                {
                    configuration = await repository.Machines
                        .RetrieveMachineConfiguration(machineDefinitionId, Context.CurrentUser.HasClaim("developer"), ct);
                }

                if (configuration == null)
                    return new NotFoundResponse
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        ReasonPhrase = "The definition was not found.",
                        ContentType = "application/json",
                        Contents = stream => stream.Close()
                    };

                //transform configuration

                

                return Negotiate
                    .WithModel(configuration)
                    .WithAllowedMediaRange(new MediaRange("application/json"));
            };

        private void GetDiagramForDefinition(IConfigurationRepositoryContextFactory configurationRepositoryContextFactory,
            IStateMachineFactory stateMachineFactory) =>
            Get["GetDiagramForDefinition", "/{MachineDefinitionId}/diagram", true] = async (parameters, ct) =>
            {
                string machineDefinitionId = parameters.MachineDefinitionId;
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