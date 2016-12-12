using Nancy;
using REstate.Configuration;
using REstate.Engine;
using REstate.Logging;
using REstate.Web.Responses;
using System;
using System.Linq;
using static Nancy.Responses.RedirectResponse;
using Nancy.Extensions;

namespace REstate.Web.Modules
{
    /// <summary>
    /// Machine interactions module.
    /// </summary>
    public class InstancesModule
        : SecuredModule
    {
        protected IPlatformLogger Logger { get; }

        protected StateEngineFactory StateEngineFactory { get; }

        /// <summary>
        /// Registers routes for interacting with machines.
        /// </summary>
        /// <param name="configurationRepositoryContextFactory">The repository context factory.</param>
        /// <param name="stateMachineFactory">The state machine factory.</param>
        /// <param name="logger"></param>
        public InstancesModule(
            REstateConfiguration configuration,
            StateEngineFactory stateEngineFactory,
            IPlatformLogger logger)
            : base(configuration, "/instances", claim => claim.Type == "claim" && claim.Value == "operator")
        {
            Logger = logger;
            StateEngineFactory = stateEngineFactory;
            

            GetMachineState();

            IsInState();

            GetAvailableTriggers();

            FireTrigger();

            GetDiagramForInstance();

            GetDiagramChartForInstance();

            GetInstanceMetadata();

            DeleteInstance();
        }

        private void GetInstanceMetadata() =>
            Get("/{MachineInstanceId}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineInstanceId = parameters.machineInstanceId;

                string metadata = await stateEngine.GetInstanceMetadataRaw(machineInstanceId, ct);

                return Negotiate
                    .WithMediaRangeResponse("application/json", Response.AsText(metadata ?? "{ }", "application/json"))
                    .WithAllowedMediaRange("application/json");
            });

        private void GetDiagramForInstance() =>
            Get("/{MachineInstanceId}/diagram", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineInstanceId = parameters.machineInstanceId;

                IStateMachine machine = await stateEngine
                    .GetInstance(machineInstanceId, ct);

                return Negotiate
                    .WithMediaRangeResponse("text/plain", Response.AsText(machine.ToString(), "text/plain"))
                    .WithAllowedMediaRange("text/plain");
            });

        private void GetDiagramChartForInstance() =>
            Get("/{MachineInstanceId}/diagram/chart", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineInstanceId = parameters.machineInstanceId;

                IStateMachine machine = await stateEngine
                    .GetInstance(machineInstanceId, ct);

                var encodedDiagram = System.Net.WebUtility.UrlEncode(machine.ToString());

                return Response.AsRedirect($"https://chart.googleapis.com/chart?chl={encodedDiagram}&cht=gv", RedirectType.SeeOther);
            });

        private void DeleteInstance() =>
            Delete("/{MachineInstanceId}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineInstanceId = parameters.machineInstanceId;

                await stateEngine.DeleteInstance(machineInstanceId, ct);

                return HttpStatusCode.Accepted;
            });

        private void FireTrigger() =>
            Post("/{MachineInstanceId}/fire/{TriggerName}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());
                
                string machineInstanceId = parameters.MachineInstanceId;
                string triggerName = parameters.TriggerName;
                var payload = this.Request.Body.AsString();

                string contentType = this.Request.Headers.ContentType;

                var commitTagHeaders = this.Request.Headers.Where(h => h.Key == "X-REstate-CommitTag");

                var commitTag = commitTagHeaders.Any() ? commitTagHeaders.First().Value.FirstOrDefault() : null;

                InstanceRecord instanceRecord;

                IStateMachine machine = await stateEngine
                    .GetInstance(machineInstanceId, ct);

                try
                {
                    await machine.FireAsync(
                        new Trigger(machine.MachineDefinitionId, triggerName),
                        contentType,
                        payload,
                        commitTag,
                        ct);
                }
                catch (InvalidOperationException ex)
                {
                    return Negotiate
                        .WithStatusCode(400)
                        .WithReasonPhrase(ex.Message)
                        .WithModel(new ReasonPhraseResponse { ReasonPhrase = ex.Message });
                }
                catch (StateConflictException ex)
                {
                    return Negotiate
                        .WithStatusCode(409)
                        .WithReasonPhrase(ex.Message)
                        .WithModel(new ReasonPhraseResponse { ReasonPhrase = ex.Message });
                }
                catch (AggregateException ex)
                    when (ex.InnerExceptions.First().GetType() == typeof(StateConflictException))
                {
                    return Negotiate
                        .WithStatusCode(409)
                        .WithReasonPhrase(ex.InnerExceptions.First().Message)
                        .WithModel(new ReasonPhraseResponse { ReasonPhrase = ex.InnerExceptions.First().Message });
                }

                instanceRecord = await stateEngine.GetInstanceInfoAsync(parameters.MachineInstanceId, ct);

                return Negotiate
                    .WithModel(instanceRecord)
                    .WithAllowedMediaRange("application/json");
            });

        private void GetAvailableTriggers() =>
            Get("/{MachineInstanceId}/triggers", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineInstanceId = parameters.MachineInstanceId;

                var machine = await stateEngine
                    .GetInstance(machineInstanceId, ct);

                var permittedTriggers = (await machine.GetPermittedTriggersAsync(ct)).Select(trigger =>
                    new Responses.Trigger
                    {
                        MachineName = trigger.MachineDefinitionId,
                        TriggerName = trigger.TriggerName
                    }).ToList();

                return Negotiate
                    .WithModel(permittedTriggers)
                    .WithAllowedMediaRange("application/json");
            });

        private void IsInState() =>
            Get("/{MachineInstanceId}/isinstate/{StateName}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineInstanceId = parameters.MachineInstanceId;
                string isInStateName = parameters.StateName;

                var machine = await stateEngine
                    .GetInstance(machineInstanceId, ct);

                var isInState = await machine.IsInStateAsync(
                    new State(machine.MachineDefinitionId, isInStateName), ct);

                return Negotiate
                    .WithModel(new IsInStateResponse 
                    {
                        QueriedStateName = isInStateName, 
                        IsInState = isInState 
                    })
                    .WithAllowedMediaRange("application/json");
                
            });

        private void GetMachineState() =>
            Get("/{MachineInstanceId}/state", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineInstanceId = parameters.MachineInstanceId;

                var instanceRecord = await stateEngine
                    .GetInstanceInfoAsync(machineInstanceId, ct);

                if (instanceRecord == null)
                    return new Response
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        ReasonPhrase = "The machine instance requested does not exist."
                    };

                return Negotiate
                    .WithModel(instanceRecord)
                    .WithAllowedMediaRange("application/json");
            });
    }
}
