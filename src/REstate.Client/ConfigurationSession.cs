using Newtonsoft.Json;
using REstate.Client.Models;
using REstate.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Client
{
    public class ConfigurationSession 
        : AuthenticatedSession, IConfigurationSession
    {
        public ConfigurationSession(Uri authBaseAddress, Uri baseAddress, string apiKey, string token)
            : base(authBaseAddress, baseAddress, apiKey, token)
        {
            
        }

        public async Task<IStateMachineConfiguration> GetStateMachineConfiguration(string machineDefinitionId)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"machinedefinitions/{machineDefinitionId}");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedException();
                    case HttpStatusCode.Forbidden:
                        throw new ForbiddenException();
                }

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
                var response = await client.PostAsync("machinedefinitions/",
                    new StringContent(payload, Encoding.UTF8, "application/json"));

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedException();
                    case HttpStatusCode.Forbidden:
                        throw new ForbiddenException();
                }

                return await response.Content.ReadAsStringAsync();
            });

            StateMachineConfiguration configurationResponse = JsonConvert.DeserializeObject<StateMachineConfigurationResponse>(responseBody);

            return configurationResponse;
        }

        public async Task<string> GetMachineDiagram(string machineDefinitionId)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"machinedefinitions/{machineDefinitionId}/diagram");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedException();
                    case HttpStatusCode.Forbidden:
                        throw new ForbiddenException();
                }

                return await response.Content.ReadAsStringAsync();
            });

            return responseBody;
        }
    }
}