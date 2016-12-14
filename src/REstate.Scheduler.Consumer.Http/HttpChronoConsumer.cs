using REstate.Logging;
using REstateClient;
using System.Threading.Tasks;
using System.Threading;

namespace REstate.Scheduler.Service
{
    public class HttpChronoConsumer : ChronoConsumer
    {
        protected REstateConfigurationClient Client { get; }

        protected IInstancesSession Session { get; set; }

        public HttpChronoConsumer(
            IChronoRepositoryFactory repositoryFactory,
            TriggerSchedulerFactory triggerSchedulerFactory,
            IPlatformLogger logger,
            REstateConfigurationClient client)
            : base(repositoryFactory, triggerSchedulerFactory, logger)
        {
            Client = client;
        }

        protected async override Task Initialize(string apiKey)
        {
            Session = await Client.GetSessionAsync(apiKey, CancellationToken.None).ConfigureAwait(false);
        }

        protected async override Task<State> GetStateAsync(string machineInstanceId, CancellationToken cancellationToken)
        {
            var record = await Session.GetInstanceInfoAsync(machineInstanceId, cancellationToken).ConfigureAwait(false);

            return new State(record.MachineName, record.StateName, record.CommitTag);
        }

        protected override Task FireTriggerAsync(string machineInstanceId, string triggerName, string contentType, string payload, string lastCommitTag, CancellationToken cancellationToken)
        {
            return Session.FireTriggerAsync(machineInstanceId, triggerName, payload, contentType, lastCommitTag, cancellationToken);
        }
    }
}
