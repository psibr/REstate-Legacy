using REstate;
using System;

namespace REstateClient
{
    public class REstateClientFactory : IREstateClientFactory
    {
        private readonly StringSerializer _stringSerializer;

        public Uri ApiKeyAuthAddress { get; }

        public REstateClientFactory(string apiKeyAuthAddress)
        {
            _stringSerializer = new SimpleJsonSerializer();

            if (string.IsNullOrWhiteSpace(apiKeyAuthAddress)) throw new ArgumentNullException(nameof(apiKeyAuthAddress));

            Uri baseUri;
            if (!Uri.TryCreate(apiKeyAuthAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(apiKeyAuthAddress));
            ApiKeyAuthAddress = baseUri;
        }

        public REstateClientFactory(StringSerializer stringSerializer, string apiKeyAuthAddress)
        {
            _stringSerializer = stringSerializer;

            if (string.IsNullOrWhiteSpace(apiKeyAuthAddress)) throw new ArgumentNullException(nameof(apiKeyAuthAddress));

            Uri baseUri;
            if (!Uri.TryCreate(apiKeyAuthAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(apiKeyAuthAddress));
            ApiKeyAuthAddress = baseUri;
        }

        public REstateClientFactory(Uri apiKeyAuthAddress)
        {
            _stringSerializer = new SimpleJsonSerializer();

            if (apiKeyAuthAddress == null) throw new ArgumentNullException(nameof(apiKeyAuthAddress));

            ApiKeyAuthAddress = apiKeyAuthAddress;
        }

        public REstateClientFactory(StringSerializer stringSerializer, Uri apiKeyAuthAddress)
        {
            _stringSerializer = stringSerializer;

            if (apiKeyAuthAddress == null) throw new ArgumentNullException(nameof(apiKeyAuthAddress));

            ApiKeyAuthAddress = apiKeyAuthAddress;
        }

        public REstateConfigurationClient GetConfigurationClient(string baseAddress)
        {
            return new REstateConfigurationClient(_stringSerializer, ApiKeyAuthAddress, baseAddress);
        }

        public REstateConfigurationClient GetConfigurationClient(Uri baseAddress)
        {
            return new REstateConfigurationClient(_stringSerializer, ApiKeyAuthAddress, baseAddress);
        }

        public REstateChronoClient GetChronoClient(string baseAddress)
        {
            return new REstateChronoClient(_stringSerializer, ApiKeyAuthAddress, baseAddress);
        }

        public REstateChronoClient GetChronoClient(Uri baseAddress)
        {
            return new REstateChronoClient(_stringSerializer, ApiKeyAuthAddress, baseAddress);
        }

        public REstateAuthClient GetAuthClient()
        {
            return new REstateAuthClient(_stringSerializer, ApiKeyAuthAddress);
        }
    }
}
