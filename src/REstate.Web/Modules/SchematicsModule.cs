using Nancy;
using Nancy.ModelBinding;
using REstate.Configuration;
using REstate.Engine;
using REstate.Logging;
using REstate.Web.Responses;
using System;
using System.Collections.Generic;
using static Nancy.Responses.RedirectResponse;

namespace REstate.Web.Modules
{
    /// <summary>
    /// Schematic configuration module.
    /// </summary>
    public class SchematicsModule
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
        public SchematicsModule(
            REstateConfiguration configuration,
            StateEngineFactory stateEngineFactory,
            IPlatformLogger logger)
            : base(configuration, "/schematics", claim => claim.Type == "claim" && claim.Value == "schematicBuilder")
        {
            Logger = logger;
            StateEngineFactory = stateEngineFactory;

            GetSchematic();

            GetSchematicDiagram();

            GetSchematicDrawing();

            CreateSchematic();

            ListSchematics();

            InstantiateMachine();

            PreviewDiagram();
        }

        private void InstantiateMachine()
        {
            Post("{SchematicName}/instantiate/", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                var metadata = this.Bind<IDictionary<string, string>>();

                string schematicName = parameters.SchematicName;

                var machineId = await stateEngine
                    .InstantiateMachine(schematicName, metadata, ct);

                return Negotiate
                    .WithModel(new MachineInstanceResponse
                        {
                            MachineId = machineId
                        })
                    .WithAllowedMediaRange("application/json");
            });
        }

        private void ListSchematics()
        {
            Get("/", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                return Negotiate
                    .WithModel(await stateEngine.ListSchematics(ct))
                    .WithAllowedMediaRange("application/json");
            });
        }

        private void CreateSchematic()
        {
            Post("/", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                var stateMachineConfiguration = this.Bind<Schematic>();

                Schematic newMachineConfiguration = await stateEngine
                    .CreateSchematic(stateMachineConfiguration, ct);

                return Negotiate
                    .WithModel(newMachineConfiguration)
                    .WithAllowedMediaRange("application/json");
            });
        }

        private void PreviewDiagram()
        {
            Post("/preview/diagram", (parameters) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                var stateMachineConfiguration = this.Bind<Schematic>();

                var diagram = stateEngine
                    .PreviewDiagram(stateMachineConfiguration);

                if (diagram == null)
                    throw new Exception("Unable to read schematic.");
                    
                return Negotiate
                    .WithMediaRangeResponse("text/plain", Response.AsText(diagram, "text/plain"))
                    .WithAllowedMediaRange("text/plain");
            });
        }

        private void PreviewDrawing()
        {
            Post("/preview/drawing", (parameters) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                var stateMachineConfiguration = this.Bind<Schematic>();

                var diagram = stateEngine
                    .PreviewDiagram(stateMachineConfiguration);

                var encodedDiagram = System.Net.WebUtility.UrlEncode(diagram);

                return Response.AsRedirect($"https://chart.googleapis.com/chart?chl={encodedDiagram}&cht=gv", RedirectType.SeeOther);
            });
        }

        private void GetSchematic()
        {
            Get("/{SchematicName}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string schematicName = parameters.SchematicName;

                try
                {
                    Schematic configuration = await stateEngine
                        .GetSchematic(schematicName, ct);

                return Negotiate
                    .WithModel(configuration)
                    .WithAllowedMediaRange("application/json");

                }
                catch(InvalidOperationException)
                {
                    return 404;
                }                
            });
        }

        private void GetSchematicDiagram()
        {
            Get("/{SchematicName}/diagram", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string schematicName = parameters.SchematicName;

                string diagram = await stateEngine
                    .GetSchematicDiagram(schematicName, ct);

                return Negotiate
                    .WithMediaRangeResponse("text/plain", Response.AsText(diagram, "text/plain"))
                    .WithAllowedMediaRange("text/plain");
            });
        }

        private void GetSchematicDrawing()
        {
            Get("/{SchematicName}/drawing", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string schematicName = parameters.SchematicName;

                string diagram = await stateEngine
                    .GetSchematicDiagram(schematicName, ct);

                var encodedDiagram = System.Net.WebUtility.UrlEncode(diagram);

                return Response.AsRedirect($"https://chart.googleapis.com/chart?chl={encodedDiagram}&cht=gv", RedirectType.SeeOther);
            });
        }
    }
}
