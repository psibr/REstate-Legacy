using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Psibr.Platform.Serialization;

namespace Psibr.Platform.Configuration.ConsulJson
{
    public class ConsulJsonConfigurationLoader<TConfiguration>
        : IConfigurationLoader<TConfiguration>
        where TConfiguration : class, IPlatformConfiguration
    {
        protected IByteSerializer ByteSerializer { get; }

        public ConsulJsonConfigurationLoader(IByteSerializer byteSerializer)
        {
            ByteSerializer = byteSerializer;

        }

        public string ConfigurationLoaderKey => "consul-json";

        public async Task<TConfiguration> Load(IDictionary<string, string> loaderConfig)
        {
            TConfiguration configuration = null;
            string serverAddress;
            string path;
            Uri serverAddressUri;

            if (loaderConfig == null 
                || !loaderConfig.TryGetValue("serverAddress", out serverAddress)
                || !loaderConfig.TryGetValue("path", out path) 
                || string.IsNullOrWhiteSpace(serverAddress) 
                || string.IsNullOrWhiteSpace(path)
                || !Uri.TryCreate(serverAddress, UriKind.Absolute, out serverAddressUri))
            {
                return configuration;
            }

            using (var client = new ConsulClient(new ConsulClientConfiguration { Address = serverAddressUri }))
            {
                var result = await client.KV.Get(path);

                if (result.StatusCode != HttpStatusCode.OK)
                    return configuration;

                var response = result.Response;

                configuration = ByteSerializer.Deserialize<TConfiguration>(response.Value);

            }

            return configuration;
        }
    }
}
