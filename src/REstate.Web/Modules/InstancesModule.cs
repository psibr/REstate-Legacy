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
            : base(configuration, "/machines", claim => claim.Type == "claim" && claim.Value == "operator")
        {
            Logger = logger;
            StateEngineFactory = stateEngineFactory;

            GetMachineState();

            IsInState();

            GetAvailableTriggers();

            FireTrigger();

            GetMachineDiagram();

            GetMachineDrawing();

            GetMachineMetadata();

            DeleteMachine();
        }

        private void GetMachineMetadata() =>
            Get("/{MachineId}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineId = parameters.MachineId;

                string metadata = await stateEngine.GetMachineMetadataRaw(machineId, ct);

                return Negotiate
                    .WithMediaRangeResponse("application/json", Response.AsText(metadata ?? "{ }", "application/json"))
                    .WithAllowedMediaRange("application/json");
            });

        private void GetMachineDiagram() =>
            Get("/{MachineId}/diagram", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineId = parameters.MachineId;

                IStateMachine machine = await stateEngine
                    .GetMachine(machineId, ct);

                return Negotiate
                    .WithMediaRangeResponse("text/plain", Response.AsText(machine.ToString(), "text/plain"))
                    .WithAllowedMediaRange("text/plain");
            });

        private void GetMachineDrawing() =>
            Get("/{MachineId}/drawing", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineId = parameters.MachineId;

                IStateMachine machine = await stateEngine
                    .GetMachine(machineId, ct);

                var encodedDiagram = System.Net.WebUtility.UrlEncode(machine.ToString());

                return Response.AsRedirect($"https://chart.googleapis.com/chart?chl={encodedDiagram}&cht=gv", RedirectType.SeeOther);
            });

        private void DeleteMachine() =>
            Delete("/{MachineId}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineId = parameters.MachineId;

                await stateEngine.DeleteMachine(machineId, ct);

                return HttpStatusCode.Accepted;
            });

        private void FireTrigger() =>
            Post("/{MachineId}/fire/{TriggerName}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());
                
                string machineId = parameters.MachineId;
                string triggerName = parameters.TriggerName;
                var payload = Request.Body.AsString();
                
                string contentType = null;
                if(Request.Headers.Keys.Contains("Content-Type"))
                    contentType = Request.Headers.ContentType;

                var commitTagHeaders = Request.Headers.Where(h => h.Key == "X-REstate-CommitTag");

                var commitTagString = commitTagHeaders.Any() ? commitTagHeaders.First().Value.FirstOrDefault() : null;

                Guid commitTagGuid;
                Guid? commitTag = null;
                if(Guid.TryParse(commitTagString, out commitTagGuid))
                {
                    commitTag = commitTagGuid;
                }

                InstanceRecord instanceRecord;

                IStateMachine machine = await stateEngine
                    .GetMachine(machineId, ct);

                State resultantState;
                try
                {
                    resultantState = await machine.FireAsync(
                        new Trigger(triggerName),
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

                instanceRecord = await stateEngine.GetMachineInfoAsync(machineId, ct);

                return Negotiate
                    .WithModel(instanceRecord)
                    .WithAllowedMediaRange("application/json");
            });

        private void GetAvailableTriggers() =>
            Get("/{MachineId}/triggers", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineId = parameters.MachineId;

                var machine = await stateEngine
                    .GetMachine(machineId, ct);

                var permittedTriggers = (await machine.GetPermittedTriggersAsync(ct)).Select(trigger =>
                    new Responses.Trigger
                    {
                        TriggerName = trigger.TriggerName
                    }).ToList();

                return Negotiate
                    .WithModel(permittedTriggers)
                    .WithAllowedMediaRange("application/json");
            });

        private void IsInState() =>
            Get("/{MachineId}/isinstate/{StateName}", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineId = parameters.MachineId;
                string isInStateName = parameters.StateName;

                var machine = await stateEngine
                    .GetMachine(machineId, ct);

                var isInState = await machine.IsInStateAsync(
                    new State(isInStateName), ct);

                return Negotiate
                    .WithModel(new IsInStateResponse 
                    {
                        QueriedStateName = isInStateName, 
                        IsInState = isInState 
                    })
                    .WithAllowedMediaRange("application/json");
                
            });

        private void GetMachineState() =>
            Get("/{MachineId}/state", async (parameters, ct) =>
            {
                var stateEngine = StateEngineFactory
                    .GetStateEngine(Context.CurrentUser?.GetApiKey());

                string machineId = parameters.MachineId;

                var instanceRecord = await stateEngine
                    .GetMachineInfoAsync(machineId, ct);

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
