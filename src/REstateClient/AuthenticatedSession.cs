using REstate;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace REstateClient
{
    public abstract class AuthenticatedSession
        : IDisposable, IAuthenticatedSession
    {
        private readonly Uri _authBaseAddress;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        protected StringSerializer StringSerializer { get; }

        protected AuthenticatedSession(StringSerializer stringSerializer, Uri authBaseAddress, Uri baseAddress, string apiKey, string token)
        {
            StringSerializer = stringSerializer;
            _authBaseAddress = authBaseAddress;
            _apiKey = apiKey;
            _httpClient = new HttpClient { BaseAddress = baseAddress };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected async Task<string> EnsureAuthenticatedRequest(Func<HttpClient, Task<string>> func)

        {
            try
            {
                return await func(_httpClient);
            }
            catch (UnauthorizedException)
            {
                var client = new REstateClientFactory(StringSerializer, _authBaseAddress).GetAuthClient();

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

        protected Exception GetException(HttpResponseMessage response)
        {
            Exception ex;
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    ex = new UnauthorizedException();
                    break;

                case HttpStatusCode.Conflict:
                    ex = new StateConflictException();
                    break;

                case HttpStatusCode.Forbidden:
                    ex = new ForbiddenException();
                    break;

                case HttpStatusCode.ServiceUnavailable:
                    ex = new HttpServiceException("Service Unavailable");
                    break;

                case HttpStatusCode.NotFound:
                    ex = new HttpServiceException("Endpoint route not found");
                    break;

                case HttpStatusCode.InternalServerError:
                    ex = new HttpServiceException("API encountered an error");
                    break;

                default:
                    ex = new Exception("Unknown error occured.");
                    break;
            }

            ex.Data.Add("StatusCode", (int)response.StatusCode);
            ex.Data.Add("Content", response.Content.ReadAsStringAsync().Result);
            ex.Data.Add("RequestUri", response.RequestMessage.RequestUri.ToString());

            return ex;
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
