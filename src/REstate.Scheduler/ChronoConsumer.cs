using REstate.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Scheduler
{
    public abstract class ChronoConsumer : IChronoConsumer
    {
        protected IPlatformLogger Logger { get; }
        protected IChronoRepositoryFactory RepositoryFactory { get; }

        private readonly TriggerSchedulerFactory _TriggerSchedulerFactory;

        public ChronoConsumer(IChronoRepositoryFactory repositoryFactory, TriggerSchedulerFactory triggerSchedulerFactory, IPlatformLogger logger)
        {
            Logger = logger;
            RepositoryFactory = repositoryFactory;

            _TriggerSchedulerFactory = triggerSchedulerFactory;
        }

        protected abstract Task<State> GetStateAsync(string machineInstanceId, CancellationToken cancellationToken);

        protected abstract Task FireTriggerAsync(string machineInstanceId, string triggerName, string contentType, string payload, string lastCommitTag, CancellationToken cancellationToken);

        protected async virtual Task Initialize(string apiKey)
        {
            await Task.CompletedTask;
        }

        public Task StartAsync(string apiKey, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                Logger.Debug("Beginning Chrono Stream.");

                var scheduler = _TriggerSchedulerFactory.GetTriggerScheduler(apiKey);
                var repository = RepositoryFactory.OpenRepository(apiKey);

                await Initialize(apiKey);

                Logger.Debug("Initialization completed.");

                foreach (var chronoTrigger in repository.GetChronoStream(cancellationToken))
                {
                    Logger.Debug("ChronoTrigger found: {@chronoTrigger}.", chronoTrigger);

                    try
                    {
                        Logger.Verbose("Checking state to ensure ChronoTrigger {{{chronoTriggerId}}} is still valid.",
                            chronoTrigger.ChronoTriggerId, chronoTrigger);

                        var currentState = await GetStateAsync(chronoTrigger.MachineInstanceId, cancellationToken);

                        if (currentState.StateName != chronoTrigger.StateName)
                        {
                            Logger.Debug(
                                "ChronoTrigger {{{chronoTriggerId}}} trigger state ({triggerState}) did not match current state ({currentState}). Removing.",
                                chronoTrigger.ChronoTriggerId, chronoTrigger.StateName, currentState.StateName);

                            await scheduler.RemoveTrigger(chronoTrigger, cancellationToken);
                        }
                        else if (chronoTrigger.VerifyCommitTag && currentState.CommitTag != chronoTrigger.LastCommitTag)
                        {
                            Logger.Debug(
                                "ChronoTrigger {{{chronoTriggerId}}} trigger state ({triggerState}) matched current state ({currentState}), " +
                                "but commit tag did not. Removing.",
                                chronoTrigger.ChronoTriggerId, chronoTrigger.StateName, currentState.StateName);

                            await scheduler.RemoveTrigger(chronoTrigger, cancellationToken);
                        }
                        else
                        {
                            Logger.Debug(
                                "ChronoTrigger {{{chronoTriggerId}}} trigger state ({triggerState}) matched current state ({currentState}). Continuing.",
                                chronoTrigger.ChronoTriggerId, chronoTrigger.StateName, currentState.StateName);

                            try
                            {
                                await FireTriggerAsync(chronoTrigger.MachineInstanceId,
                                    chronoTrigger.TriggerName, chronoTrigger.ContentType, chronoTrigger.Payload, chronoTrigger.VerifyCommitTag ? chronoTrigger.LastCommitTag : null, cancellationToken);

                                await scheduler.RemoveTrigger(chronoTrigger, cancellationToken);

                                Logger.Information("ChronoTrigger {{{chronoTriggerId}}} fired successfully.",
                                    chronoTrigger.ChronoTriggerId, chronoTrigger);
                            }
                            catch (StateConflictException ex)
                            {
                                Logger.Debug(ex, "State conflict occured on ChronoTrigger {{{chronoTriggerId}}} when firing.",
                                    chronoTrigger.ChronoTriggerId, chronoTrigger);
                            }
                            catch (AggregateException ex)
                                when (ex.InnerExceptions.First().GetType() == typeof(StateConflictException))
                            {
                                Logger.Debug(ex.InnerExceptions.First(), "State conflict occured on ChronoTrigger {{{chronoTriggerId}}} when firing.",
                                    chronoTrigger.ChronoTriggerId, chronoTrigger);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Unable to check current state.");
                    }
                }
            }, CancellationToken.None);
        }
    }
}
