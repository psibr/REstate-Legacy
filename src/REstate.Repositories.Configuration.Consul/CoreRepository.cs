using System;
using System.Linq;
using Consul;
using Psibr.Platform;
using Psibr.Platform.Serialization;
using REstate.Repositories.Configuration;
using REstate.Repositories.Instances;

namespace REstate.Repositories.Core.Consul
{
    public class CoreRepository
        : IConfigurationRepository, IInstanceRepository
    {
        public CoreRepository(IPlatformConfiguration platformConfiguration, IByteSerializer serializer, string apiKey)
        {
            PlatformConfiguration = platformConfiguration;

            if (platformConfiguration == null) throw new ArgumentNullException(nameof(platformConfiguration));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            Serializer = serializer;
            var connection = platformConfiguration.Connections.Single(c => c.Tags.Contains("configuration") && c.ProviderName == "consul");

            string restateRootPath;
            connection.AdditionalOptions.TryGetValue("restateRootPath", out restateRootPath);

            if (restateRootPath != null)
                REstateRootPath = restateRootPath.TrimEnd('/');

            Client = new ConsulClient(new ConsulClientConfiguration
            {
                Address = new Uri(connection.ConnectionString)
            });

            ApiKey = apiKey;

            Machines = new MachineConfigurationRepository(this);
            Code = new CodeConfigurationRepository(this);
            MachineInstances = new MachineInstancesRepository(this);
        }

        public string REstateRootPath { get; }

        public IByteSerializer Serializer { get; }

        IConfigurationRepository IConfigurationContextualRepository.Root => this;

        IInstanceRepository IInstanceContextualRepository.Root => this;

        /// <summary>
        /// Gets the API key.
        /// </summary>
        /// <value>The API key.</value>
        public string ApiKey { get; }

        /// <summary>
        /// Gets the database manager.
        /// </summary>
        /// <value>The database manager.</value>
        public ConsulClient Client { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Client.Dispose();
        }

        public IMachineConfigurationRepository Machines { get; }

        public ICodeConfigurationRepository Code { get; }

        public IPlatformConfiguration PlatformConfiguration { get; }

        public IMachineInstancesRepository MachineInstances { get; }
    }
}