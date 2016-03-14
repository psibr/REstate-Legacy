using System;
using Microsoft.Owin.Hosting;
using REstate.Logging;
using REstate.Web;

namespace REstate.Services.Common.Api
{
    public class REstateApiService<TOwinStartup>
        : IDisposable
    {
        private IREstateLogger Logger { get; }
        private REstateConfiguration Configuration { get; }

        private IDisposable _webApp;

        public REstateApiService(REstateConfiguration configuration, IREstateLogger logger)
        {
            Logger = logger;
            Configuration = configuration;
        }

        public void Start()
        {
            _webApp = WebApp.Start<TOwinStartup>(Configuration.HostBindingAddress);

            Logger.Information("Running at {hostBindingAddress}", Configuration.HostBindingAddress);
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
