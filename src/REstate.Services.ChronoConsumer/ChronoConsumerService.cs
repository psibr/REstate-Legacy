using System;
using REstate.Chrono;
using REstate.Client;
using REstate.Logging;
using REstate.Web;

namespace REstate.Services.ChronoConsumer
{
    public class ChronoConsumerService
        : IDisposable
    {
        private IAuthSessionClient<IInstancesSession> InstanceSessionClient { get; }
        private IInstancesSession _instanceSession;
        private IREstateLogger Logger { get; }
        private REstateConfiguration Configuration { get; }
        private IChronoRepository ChronoRepository { get; }
        private Repositories.Chrono.Susanoo.ChronoConsumer _chronoConsumer;

        public ChronoConsumerService(REstateConfiguration configuration, IChronoRepository chronoRepository, 
            IAuthSessionClient<IInstancesSession> instanceSessionClient, IREstateLogger logger)
        {
            InstanceSessionClient = instanceSessionClient;
            Logger = logger;
            Configuration = configuration;
            ChronoRepository = chronoRepository;
        }

        public void Start()
        {
            _instanceSession = InstanceSessionClient
                .GetSession(Configuration.ConfigurationDictionary["ApiKey"]).Result;

            Logger.Information("Authenticated session acquired.");

            _chronoConsumer = new Repositories.Chrono.Susanoo.ChronoConsumer(ChronoRepository, _instanceSession);

            _chronoConsumer.Start();

            Logger.Information("Starting ChronoConsumer.");

        }

        public void Stop()
        {
            _chronoConsumer.Stop();
        }

        public void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _chronoConsumer?.Stop();
                
                _instanceSession.Dispose();
            }

            _chronoConsumer = null;
            _instanceSession = null;
        }
        public void Dispose()
        {
            Dispose(true);
        }

        ~ChronoConsumerService()
        {
            Dispose(false);
        }
    }
}

