using System;
using Microsoft.Owin.Hosting;
using REstate.Logging;
using REstate.Platform;

namespace REstate.ApiService
{
    public class REstateApiService<TOwinStartup>
        : IDisposable
    {
        private REstateApiServiceConfiguration ApiServiceConfiguration { get; }
        private IREstateLogger Logger { get; }

        private IDisposable _webApp;

        public REstateApiService(REstateApiServiceConfiguration apiServiceConfiguration, IREstateLogger logger)
        {
            ApiServiceConfiguration = apiServiceConfiguration;
            Logger = logger;
        }

        public void Start()
        {
            _webApp = WebApp.Start<TOwinStartup>(ApiServiceConfiguration.HostBindingAddress);

            Logger.Information("Running at {hostBindingAddress}", ApiServiceConfiguration.HostBindingAddress);
        }

        public void Stop()
        {
            _webApp.Dispose();
            _webApp = null;
        }

        public void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _webApp?.Dispose();
                _webApp = null;
            }

            _webApp = null;
        }
        public void Dispose()
        {
            Dispose(true);
        }

        ~REstateApiService()
        {
            Dispose(false);
        }
    }
}
