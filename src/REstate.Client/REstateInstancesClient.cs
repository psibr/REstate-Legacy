using System;
using System.Threading.Tasks;

namespace REstate.Client
{
    public class REstateInstancesClient
        : REstateAuthClient, IAuthSessionClient<IInstancesSession>
    {
        protected readonly Uri BaseAddress;

        public REstateInstancesClient(string authServiceAddress, string baseAddress)
            : base(authServiceAddress)
        {
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentNullException(nameof(baseAddress));

            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        public REstateInstancesClient(Uri authServiceAddress, string baseAddress)
            : base(authServiceAddress)
        {
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentNullException(nameof(baseAddress));

            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        public REstateInstancesClient(Uri authServiceAddress, Uri baseAddress)
            : base(authServiceAddress)
        {
            BaseAddress = baseAddress;
        }

        public REstateInstancesClient(string authServiceAddress, Uri baseAddress)
            : base(authServiceAddress)
        {
            BaseAddress = baseAddress;
        }

        public async Task<IInstancesSession> GetSession(string apiKey)
        {
            var token = await GetAuthenticatedSessionToken(apiKey);

            return new InstancesSession(ApiKeyAuthAddress, BaseAddress, apiKey, token);
        }
    }
}