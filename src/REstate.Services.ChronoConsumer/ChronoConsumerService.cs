using System;
using Psibr.Platform.Logging;
using REstate.Chrono;
using REstate.Client;

namespace REstate.Services.ChronoConsumer
{
    public class ConsumerServiceConfiguration
    {
        public string ApiKey { get; set; }
    }

    public class ChronoConsumerService
        : IDisposable
    {
        private IAuthSessionClient<IInstancesSession> InstanceSessionClient { get; }
        private IInstancesSession _instanceSession;
        private IPlatformLogger Logger { get; }
        private ConsumerServiceConfiguration Configuration { get; }
        private IChronoRepository ChronoRepository { get; }
        private Repositories.Chrono.Susanoo.ChronoConsumer _chronoConsumer;

        public ChronoConsumerService(ConsumerServiceConfiguration configuration, IChronoRepository chronoRepository, 
            IAuthSessionClient<IInstancesSession> instanceSessionClient, IPlatformLogger logger)
        {
            InstanceSessionClient = instanceSessionClient;
            Logger = logger;
            Configuration = configuration;
            ChronoRepository = chronoRepository;
        }

        public void Start()
        {
            _instanceSession = InstanceSessionClient
                .GetSession(Configuration.ApiKey).Result;

            Logger.Information("Authenticated session acquired.");

            _chronoConsumer = new Repositories.Chrono.Susanoo.ChronoConsumer(ChronoRepository, _instanceSession,
                Logger.ForContext<Repositories.Chrono.Susanoo.ChronoConsumer>());

            Logger.Information("Starting ChronoConsumer.");

            _chronoConsumer.Start();



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

