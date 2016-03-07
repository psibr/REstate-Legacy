using System;

namespace REstate.Client
{
    public class REstateClientFactory : IREstateClientFactory
    {
        public REstateClientFactory(string apiKeyAuthAddress)
        {
            if (string.IsNullOrWhiteSpace(apiKeyAuthAddress)) throw new ArgumentNullException(nameof(apiKeyAuthAddress));

            Uri baseUri;
            if (!Uri.TryCreate(apiKeyAuthAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(apiKeyAuthAddress));
            ApiKeyAuthAddress = baseUri;
        }

        public REstateClientFactory(Uri apiKeyAuthAddress)
        {

            if (apiKeyAuthAddress == null) throw new ArgumentNullException(nameof(apiKeyAuthAddress));

            ApiKeyAuthAddress = apiKeyAuthAddress;
        }

        public Uri ApiKeyAuthAddress { get; }

        public REstateConfigurationClient GetConfigurationClient(string baseAddress)
        {
            return new REstateConfigurationClient(ApiKeyAuthAddress, baseAddress);
        }

        public REstateConfigurationClient GetConfigurationClient(Uri baseAddress)
        {
            return new REstateConfigurationClient(ApiKeyAuthAddress, baseAddress);
        }

        public REstateAuthClient GetAuthClient()
        {
            return new REstateAuthClient(ApiKeyAuthAddress);
        }

        public REstateInstancesClient GetInstancesClient(string baseAddress)
        {
            return new REstateInstancesClient(ApiKeyAuthAddress, baseAddress);
        }

        public REstateInstancesClient GetInstancesClient(Uri baseAddress)
        {
            return new REstateInstancesClient(ApiKeyAuthAddress, baseAddress);
        }
    }
}