using REstate.Chrono;
using REstate.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Psibr.Platform.Logging;

namespace REstate.Repositories.Chrono.Susanoo
{
    public class ChronoConsumer : IChronoConsumer
    {
        protected IPlatformLogger Logger { get; }
        private readonly IChronoRepository _repository;
        private readonly IInstancesSession _clientAuthenticatedSession;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public ChronoConsumer(IChronoRepository repository, IInstancesSession clientAuthenticatedSession, IPlatformLogger logger)
        {
            Logger = logger;
            _repository = repository;
            _clientAuthenticatedSession = clientAuthenticatedSession;
        }

        public void Start() => Task.Run(() =>
        {
            Logger.Debug("Beginning Chrono Stream.");
            foreach (var chronoTrigger in _repository.GetChronoStream(_cts.Token))
            {
                Logger.Debug("ChronoTrigger found: {@chronoTrigger}", chronoTrigger);

                try
                {
                    Logger.Verbose("Checking state to ensure ChronoTrigger {{{chronoTriggerId}}} is still valid.",
                        chronoTrigger.ChronoTriggerId, chronoTrigger);

                    var currentState = _clientAuthenticatedSession
                        .GetState(chronoTrigger.MachineInstanceId).Result;

                    if (currentState.StateName != chronoTrigger.StateName)
                    {
                        Logger.Debug(
                            "ChronoTrigger {{{chronoTriggerId}}} trigger state ({triggerState}) did not match current state ({currentState}). Removing.",
                            chronoTrigger.ChronoTriggerId, chronoTrigger.StateName, currentState.StateName);

                        _repository.RemoveChronoTrigger(chronoTrigger, _cts.Token).Wait();
                    }
                    else
                    {
                        Logger.Debug(
                            "ChronoTrigger {{{chronoTriggerId}}} trigger state ({triggerState}) matched current state ({currentState}). Continuing.",
                            chronoTrigger.ChronoTriggerId, chronoTrigger.StateName, currentState.StateName);

                        try
                        {
                            _clientAuthenticatedSession.FireTrigger(chronoTrigger.MachineInstanceId,
                                chronoTrigger.TriggerName, chronoTrigger.Payload).Wait();

                            _repository.RemoveChronoTrigger(chronoTrigger, _cts.Token).Wait();

                            Logger.Information("ChronoTrigger {{{chronoTriggerId}}} fired successfully.",
                                chronoTrigger.ChronoTriggerId, chronoTrigger);
                        }
                        catch (StateConflictException ex)
                        {
                            Logger.Debug(ex, "State conflict occured on ChronoTrigger {{{chronoTriggerId}}} firing.",
                                chronoTrigger.ChronoTriggerId, chronoTrigger);
                        }
                        catch (AggregateException ex)
                            when (ex.InnerExceptions.First().GetType() == typeof(StateConflictException))
                        {
                            Logger.Debug(ex.InnerExceptions.First(), "State conflict occured on ChronoTrigger {{{chronoTriggerId}}} firing.",
                                chronoTrigger.ChronoTriggerId, chronoTrigger);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to check current state.");
                }

            }
        }, _cts.Token);

        public void Stop()
        {
            _cts.Cancel();
        }
    }
}