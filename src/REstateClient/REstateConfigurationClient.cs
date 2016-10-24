using System;
using System.Threading.Tasks;

namespace REstateClient
{
    public class REstateConfigurationClient
        : REstateAuthClient, IAuthSessionClient<IConfigurationSession>
    {
        protected readonly Uri BaseAddress;

        public REstateConfigurationClient(string authServiceAddress, string baseAddress)
            : base(authServiceAddress)
        {
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentNullException(nameof(baseAddress));

            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        public REstateConfigurationClient(Uri authServiceAddress, string baseAddress)
            : base(authServiceAddress)
        {
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentNullException(nameof(baseAddress));

            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        public REstateConfigurationClient(Uri authServiceAddress, Uri baseAddress)
            : base(authServiceAddress)
        {
            BaseAddress = baseAddress;
        }

        public REstateConfigurationClient(string authServiceAddress, Uri baseAddress)
            : base(authServiceAddress)
        {
            BaseAddress = baseAddress;
        }

        public async Task<IConfigurationSession> GetSession(string apiKey)
        {
            var token = await GetAuthenticatedSessionToken(apiKey);

            return new ConfigurationSession(ApiKeyAuthAddress, BaseAddress, apiKey, token);
        }
    }
}
