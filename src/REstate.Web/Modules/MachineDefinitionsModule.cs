using Nancy;
using Nancy.ModelBinding;
using REstate.Configuration;
using REstate.Engine;
using REstate.Logging;
using REstate.Web.Responses;
using System;
using System.Collections.Generic;

namespace REstate.Web.Modules
{
    /// <summary>
    /// Machine Definitions configuration module.
    /// </summary>
    public class MachineDefinitionsModule
        : SecuredModule
    {
        protected IPlatformLogger Logger { get; }
        protected StateEngineFactory StateEngineFactory { get; }

        /// <summary>
        /// Registers routes for defining new machines.
        /// </summary>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        /// <param name="logger"></param>
        public MachineDefinitionsModule(
            StateEngineFactory stateEngineFactory,
            IPlatformLogger logger)
            : base("/machines", claim => claim.Type == "claim" && claim.Value == "machineBuilder")
        {
            Logger = logger;
            StateEngineFactory = stateEngineFactory;

            GetMachine();

            GetDiagramForDefinition();

            DefineStateMachine();

            ListMachines();

            InstantiateMachine();

            PreviewDiagram();
        }

        private void InstantiateMachine()
        {
            Post("{MachineDefinitionId}/instantiate/", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser.GetApiKey());

                var metadata = this.Bind<IDictionary<string, string>>();

                string machineDefinitionId = parameters.MachineDefinitionId;

                var machineInstanceId = await stateEngine
                    .InstantiateMachine(machineDefinitionId, metadata, ct);

                return new MachineInstanceResponse
                {
                    MachineInstanceId = machineInstanceId
                };
            });
        }

        private void ListMachines()
        {
            Get("/", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser.GetApiKey());

                return await stateEngine.ListMachines(ct);
            });
        }

        private void DefineStateMachine()
        {
            Post("/", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser.GetApiKey());

                var stateMachineConfiguration = this.Bind<Machine>();

                Machine newMachineConfiguration = await stateEngine
                    .DefineStateMachine(stateMachineConfiguration, ct);

                return newMachineConfiguration;
            });
        }

        private void PreviewDiagram()
        {
            Post("/preview", (parameters) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser.GetApiKey());

                var stateMachineConfiguration = this.Bind<Machine>();

                var machine = stateEngine
                    .PreviewDiagram(stateMachineConfiguration);

                if (machine == null)
                    throw new Exception("Unable to construct machine.");

                return Response.AsText(machine.ToString(), "text/plain");
            });
        }

        private void GetMachine()
        {
            Get("/{MachineDefinitionId}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser.GetApiKey());

                string machineDefinitionId = parameters.MachineDefinitionId;

                try
                {
                    Machine configuration = await stateEngine
                        .GetMachineDefinition(machineDefinitionId, ct);

                    return configuration;
                }
                catch(InvalidOperationException)
                {
                    return 404;
                }                
            });
        }

        private void GetDiagramForDefinition()
        {
            Get("/{MachineDefinitionId}/diagram", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser.GetApiKey());

                string machineDefinitionId = parameters.MachineDefinitionId;

                string diagram = await stateEngine
                    .GetDiagramForDefinition(machineDefinitionId, ct);

                return Response.AsText(diagram, "text/plain");
            });
        }
    }
}
