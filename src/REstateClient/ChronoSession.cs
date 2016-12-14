using REstate;
using REstate.Scheduling;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace REstateClient
{
    public class ChronoSession
        : AuthenticatedSession, IChronoSession
    {
        public ChronoSession(StringSerializer stringSerializer, Uri apiKeyAuthAddress, Uri baseAddress, string apiKey, string token)
            : base(stringSerializer, apiKeyAuthAddress, baseAddress, apiKey, token)
        {
        }

        public async Task AddChronoTriggerAsync(ChronoTrigger chronoTrigger, CancellationToken cancellationToken)
        {
            var payload = StringSerializer.Serialize(chronoTrigger);

            await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.PostAsync("triggers",
                    new StringContent(payload, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
