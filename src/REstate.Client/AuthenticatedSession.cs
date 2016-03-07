using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace REstate.Client
{ 
    public abstract class AuthenticatedSession
        : IDisposable, IAuthenticatedSession
    {
        private readonly Uri _authBaseAddress;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        protected AuthenticatedSession(Uri authBaseAddress, Uri baseAddress, string apiKey, string token)
        {
            _authBaseAddress = authBaseAddress;
            _apiKey = apiKey;
            _httpClient = new HttpClient { BaseAddress = baseAddress };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        protected async Task<string> EnsureAuthenticatedRequest(Func<HttpClient, Task<string>> func)
        {
            try
            {
                return await func(_httpClient);
            }
            catch (UnauthorizedException)
            {
                var client = new REstateClientFactory(_authBaseAddress).GetAuthClient();

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", await client.GetAuthenticatedSessionToken(_apiKey));

                return await func(_httpClient);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }

        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        ~AuthenticatedSession()
        {
            Dispose(false);
        }
    }
}
