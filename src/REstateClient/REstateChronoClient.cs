using REstate;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace REstateClient
{
    public class REstateChronoClient
        : REstateAuthClient, IAuthSessionClient<IChronoSession>
    {

        public REstateChronoClient(StringSerializer stringSerializer, string apiKeyAuthAddress, string baseAddress)
            : base(stringSerializer, apiKeyAuthAddress)
        {
            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        protected readonly Uri BaseAddress;

        public REstateChronoClient(StringSerializer stringSerializer, Uri apiKeyAuthAddress, string baseAddress)
            : base(stringSerializer, apiKeyAuthAddress)
        {
            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));

            BaseAddress = baseUri;
        }

        public REstateChronoClient(StringSerializer stringSerializer, string apiKeyAuthAddress, Uri baseAddress)
            : base(stringSerializer, apiKeyAuthAddress)
        {
            BaseAddress = baseAddress;
        }

        public REstateChronoClient(StringSerializer stringSerializer, Uri apiKeyAuthAddress, Uri baseAddress)
            : base(stringSerializer, apiKeyAuthAddress)
        {
            BaseAddress = baseAddress;
        }

        public async Task<IChronoSession> GetSessionAsync(string apiKey, CancellationToken cancellationToken)
        {
            var token = await GetAuthenticatedSessionTokenAsync(apiKey, cancellationToken).ConfigureAwait(false);

            return new ChronoSession(StringSerializer, ApiKeyAuthAddress, BaseAddress, apiKey, token);
        }
    }
}
