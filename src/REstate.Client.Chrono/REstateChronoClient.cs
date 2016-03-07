using System;
using System.Threading.Tasks;

namespace REstate.Client.Chrono
{
    public class REstateChronoClient
        : REstateAuthClient, IAuthSessionClient<IChronoSession>
    {
        public REstateChronoClient(string apiKeyAuthAddress, string baseAddress) 
            : base(apiKeyAuthAddress)
        {
            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        protected readonly Uri BaseAddress;

        public REstateChronoClient(Uri apiKeyAuthAddress, string baseAddress) 
            : base(apiKeyAuthAddress)
        {
            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        public REstateChronoClient(string apiKeyAuthAddress, Uri baseAddress)
            : base(apiKeyAuthAddress)
        {
            BaseAddress = baseAddress;
        }

        public REstateChronoClient(Uri apiKeyAuthAddress, Uri baseAddress)
            : base(apiKeyAuthAddress)
        {
            BaseAddress = baseAddress;
        }

        public async Task<IChronoSession> GetSession(string apiKey)
        {
            var token = await GetAuthenticatedSessionToken(apiKey);

            return new ChronoSession(ApiKeyAuthAddress, BaseAddress, apiKey, token);
        }
    }
}
