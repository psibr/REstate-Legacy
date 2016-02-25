using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using REstate.Client.Models;
using REstate.Configuration;

namespace REstate.Client
{
    public class AuthenticatedSession
        : IDisposable
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        internal AuthenticatedSession(Uri baseAddress, string apiKey, string token)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient { BaseAddress = baseAddress };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<IStateMachineConfiguration> GetStateMachineConfiguration(int machineDefinitionId)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"configuration/machinedefinitions/{machineDefinitionId}");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            StateMachineConfiguration configuration = JsonConvert.DeserializeObject<StateMachineConfigurationResponse>(responseBody);

            return configuration;
        }

        public async Task<IStateMachineConfiguration> DefineStateMachine(IStateMachineConfiguration configuration)
        {
            var payload = JsonConvert.SerializeObject(configuration);

            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.PostAsync("configuration/machinedefinitions/",
                    new StringContent(payload, Encoding.UTF8, "application/json"));

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            StateMachineConfiguration configurationResponse = JsonConvert.DeserializeObject<StateMachineConfigurationResponse>(responseBody);

            return configurationResponse;
        }

        public async Task<string> GetMachineDiagram(int machineDefinitionId)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"configuration/machinedefinitions/{machineDefinitionId}/diagram");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            return responseBody;
        }

        public async Task<Guid> InstantiateMachine(int machineDefinitionId)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.PostAsync($"machines/instantiate/{machineDefinitionId}", 
                    new StringContent(string.Empty));

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            var instance = JsonConvert.DeserializeObject<MachineInstantiateResponse>(responseBody);

            return instance.MachineInstanceGuid;
        }

        public async Task<State> GetMachineState(Guid machineInstanceGuid)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineInstanceGuid}/state");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            State state = JsonConvert.DeserializeObject<StateModel>(responseBody);

            return state;
        }

        public async Task<bool> IsMachineInState(Guid machineInstanceGuid, string stateName)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineInstanceGuid}/isinstate/{stateName}");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            var isInStateResponse = JsonConvert.DeserializeObject<IsInStateResponse>(responseBody);

            return isInStateResponse.IsInState;
        }

        public async Task<ICollection<REstate.Trigger>> GetAvailableTriggers(Guid machineInstanceGuid)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineInstanceGuid}/triggers");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            var triggers = JsonConvert.DeserializeObject<TriggerModel[]>(responseBody);

            return triggers.Select(t => (REstate.Trigger)t).ToArray();
        }

        public async Task<REstate.State> FireTrigger(Guid machineInstanceGuid, string triggerName, string payload = null)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.PostAsync($"machines/{machineInstanceGuid}/fire/{triggerName}",
                    payload == null ? new StringContent(string.Empty) : 
                        new StringContent($"{{ \"payload\": \"{payload}\" }}", Encoding.UTF8, "application/json"));


                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedException();
                    case HttpStatusCode.Conflict:
                        throw new StateConflictException();
                }

                return await response.Content.ReadAsStringAsync();
            });

            REstate.State state = JsonConvert.DeserializeObject<StateModel>(responseBody);

            return state;
        }

        public async Task DeleteInstance(Guid machineInstanceGuid)
        {
            await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.DeleteAsync($"machines/{machineInstanceGuid}");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return null;
            });
        }

        public async Task<string> GetMachineDiagram(Guid machineInstanceGuid)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineInstanceGuid}/diagram");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });
            
            return responseBody;
        }

        protected async Task<string> EnsureAuthenticatedRequest(Func<HttpClient, Task<string>> func)
        {
            try
            {
                return await func(_httpClient);
            }
            catch (UnauthorizedException)
            {
                var client = new REstateClient(_httpClient.BaseAddress);

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
