using REstate;
using REstate.Configuration;
using REstateClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace REstateClient
{
    public class ConfigurationSession
        : AuthenticatedSession, IConfigurationSession
    {
        public ConfigurationSession(StringSerializer stringSerializer, Uri authBaseAddress, Uri baseAddress, string apiKey, string token)
            : base(stringSerializer, authBaseAddress, baseAddress, apiKey, token)
        {
        }

        public async Task<string> InstantiateAsync(string schematicName, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.PostAsync($"schematics/{schematicName}/instantiate",
                    new StringContent(string.Empty)).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return (await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }, cancellationToken).ConfigureAwait(false);

            var instance = StringSerializer.Deserialize<MachineInstantiateResponse>(responseBody);

            return instance.MachineId;
        }

        public async Task<State> GetStateAsync(string machineId, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineId}/state").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            State state = StringSerializer.Deserialize<StateModel>(responseBody);

            return state;
        }

        public async Task<InstanceRecord> GetInstanceInfoAsync(string machineId, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineId}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            InstanceRecord info = StringSerializer.Deserialize<InstanceRecord>(responseBody);

            return info;
        }

        public async Task<bool> IsInStateAsync(string machineId, string stateName, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineId}/isinstate/{stateName}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            var isInStateResponse = StringSerializer.Deserialize<IsInStateResponse>(responseBody);

            return isInStateResponse.IsInState;
        }

        public async Task<ICollection<Trigger>> GetAvailableTriggersAsync(string machineId, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineId}/triggers").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            var triggers = StringSerializer.Deserialize<TriggerModel[]>(responseBody);

            return triggers.Select(t => (Trigger)t).ToArray();
        }

        public async Task<State> FireTriggerAsync(string machineId, string triggerName, string contentType, string payload, Guid? commitTag, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var content = payload == null
                        ? new StringContent(string.Empty)
                        : new StringContent(payload, Encoding.UTF8, contentType);

                if(commitTag != null)
                    content.Headers.Add("X-REstate-CommitTag", new [] { commitTag.ToString() });

                var response = await client.PostAsync(
                    $"machines/{machineId}/fire/{triggerName}",
                    content).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            StateModel state = StringSerializer.Deserialize<StateModel>(responseBody);

            return state;
        }

        public async Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.DeleteAsync($"{machineId}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return null; //TODO: make Task overload
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetMachineDiagramAsync(string machineId, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineId}/diagram").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            return responseBody;
        }

        public async Task<Schematic> GetSchematicAsync(string schematicName, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"schematics/{schematicName}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    throw GetException(response);
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            var configuration = responseBody != null ? StringSerializer.Deserialize<Schematic>(responseBody) : null;

            return configuration;
        }

        public async Task<Schematic> CreateSchematicAsync(Schematic configuration, CancellationToken cancellationToken)
        {
            var payload = StringSerializer.Serialize(configuration);

            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {

                var response = await client.PostAsync("schematics/",
                    new StringContent(payload, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            var configurationResponse = StringSerializer.Deserialize<Schematic>(responseBody);

            return configurationResponse;
        }

        public async Task<string> GetSchematicDiagramAsync(string schematicName, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"schematics/{schematicName}/diagram").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            return responseBody;
        }
    }
}
