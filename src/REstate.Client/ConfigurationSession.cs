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
    }
}