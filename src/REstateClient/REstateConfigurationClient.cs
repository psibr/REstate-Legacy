using REstate;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace REstateClient
{
    public class REstateConfigurationClient
        : REstateAuthClient, IAuthSessionClient<IConfigurationSession>
    {
        protected readonly Uri BaseAddress;

        public REstateConfigurationClient(StringSerializer stringSerializer, string authServiceAddress, string baseAddress)
            : base(stringSerializer, authServiceAddress)
        {

            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentNullException(nameof(baseAddress));

            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        public REstateConfigurationClient(StringSerializer stringSerializer, Uri authServiceAddress, string baseAddress)
            : base(stringSerializer, authServiceAddress)
        {
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentNullException(nameof(baseAddress));

            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        public REstateConfigurationClient(StringSerializer stringSerializer, Uri authServiceAddress, Uri baseAddress)
            : base(stringSerializer, authServiceAddress)
        {
            BaseAddress = baseAddress;
        }

        public REstateConfigurationClient(StringSerializer stringSerializer, string authServiceAddress, Uri baseAddress)
            : base(stringSerializer, authServiceAddress)
        {
            BaseAddress = baseAddress;
        }

        public async Task<IConfigurationSession> GetSessionAsync(string apiKey, CancellationToken cancellationToken)
        {
            var token = await GetAuthenticatedSessionTokenAsync(apiKey, cancellationToken).ConfigureAwait(false);

            return new ConfigurationSession(StringSerializer, ApiKeyAuthAddress, BaseAddress, apiKey, token);
        }
    }
}
