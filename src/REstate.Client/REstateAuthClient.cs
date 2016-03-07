using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using REstate.Client.Models;

namespace REstate.Client
{
    public class REstateAuthClient
    {
        protected readonly Uri ApiKeyAuthAddress;
        private const string Json = "application/json";

        public REstateAuthClient(string apiKeyAuthAddress)
        {
            if (string.IsNullOrWhiteSpace(apiKeyAuthAddress)) throw new ArgumentNullException(nameof(apiKeyAuthAddress));

            Uri baseUri;
            if (!Uri.TryCreate(apiKeyAuthAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(apiKeyAuthAddress));
            ApiKeyAuthAddress = baseUri;
        }

        public REstateAuthClient(Uri apiKeyAuthAddress)
        {

            if (apiKeyAuthAddress == null) throw new ArgumentNullException(nameof(apiKeyAuthAddress));

            ApiKeyAuthAddress = apiKeyAuthAddress;
        }

        public async Task<string> GetAuthenticatedSessionToken(string apiKey)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(ApiKeyAuthAddress,
                    new StringContent($"{{ \"apiKey\": \"{apiKey}\" }}", Encoding.UTF8, Json));

                if (!response.IsSuccessStatusCode) return null;

                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JwtResponse>(responseBody);

                return tokenResponse.Jwt;
            }
        }
    }
}
