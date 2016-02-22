using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using REstate.Client.Models;

namespace REstate.Client
{
    public class REstateClient
    {
        private readonly Uri _baseAddress;
        private const string Json = "application/json";

        public REstateClient(string baseAddress)
        {
            if (string.IsNullOrWhiteSpace(baseAddress)) throw new ArgumentNullException(nameof(baseAddress));

            Uri baseUri;
            if (!Uri.TryCreate(baseAddress, UriKind.RelativeOrAbsolute, out baseUri)) throw new ArgumentException("Not a valid Uri", nameof(baseAddress));
            _baseAddress = baseUri;
        }

        public REstateClient(Uri baseAddress)
        {
            
            if (baseAddress == null) throw new ArgumentNullException(nameof(baseAddress));

            _baseAddress = baseAddress;
        }

        public async Task<AuthenticatedSession> GetAuthenticatedSession(string apiKey)
        {
            var token = await GetAuthenticatedSessionToken(apiKey);

            return new AuthenticatedSession(_baseAddress, apiKey, token);
        }

        internal async Task<string> GetAuthenticatedSessionToken(string apiKey)
        {
            using (var httpClient = new HttpClient {BaseAddress = _baseAddress})
            {
                var response = await httpClient.PostAsync("apikey",
                    new StringContent($"{{ \"apiKey\": \"{apiKey}\" }}", Encoding.UTF8, Json));

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JwtResponse>(responseBody);

                return tokenResponse.Jwt;
            }
        }
    }
}
