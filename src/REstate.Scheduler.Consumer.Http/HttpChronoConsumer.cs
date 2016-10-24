using REstate.Configuration;
using REstate.Logging;
using REstateClient;
using System.Threading.Tasks;

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
            Session = await Client.GetSession(apiKey);
        }

        protected async override Task<InstanceRecord> GetState(string machineInstanceId)
        {
            return await Session.GetInstanceInfo(machineInstanceId);
        }

        protected async override Task FireTrigger(string machineInstanceId, string triggerName, string payload)
        {
            await Session.FireTrigger(machineInstanceId, triggerName, payload);
        }
    }
}
