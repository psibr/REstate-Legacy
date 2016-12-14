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

        public async Task<string> InstantiateAsync(string machineName, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.PostAsync($"machines/{machineName}/instantiate",
                    new StringContent(string.Empty)).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return (await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }, cancellationToken).ConfigureAwait(false);

            var instance = StringSerializer.Deserialize<MachineInstantiateResponse>(responseBody);

            return instance.MachineInstanceId;
        }

        public async Task<State> GetStateAsync(string instanceId, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"instances/{instanceId}/state").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            State state = StringSerializer.Deserialize<StateModel>(responseBody);

            return state;
        }

        public async Task<InstanceRecord> GetInstanceInfoAsync(string instanceId, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"instances/{instanceId}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            InstanceRecord info = StringSerializer.Deserialize<InstanceRecord>(responseBody);

            return info;
        }

        public async Task<bool> IsInStateAsync(string instanceId, string stateName, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"instances/{instanceId}/isinstate/{stateName}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            var isInStateResponse = StringSerializer.Deserialize<IsInStateResponse>(responseBody);

            return isInStateResponse.IsInState;
        }

        public async Task<ICollection<Trigger>> GetAvailableTriggersAsync(string instanceId, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"instances/{instanceId}/triggers").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            var triggers = StringSerializer.Deserialize<TriggerModel[]>(responseBody);

            return triggers.Select(t => (Trigger)t).ToArray();
        }

        public async Task<State> FireTriggerAsync(string instanceId, string triggerName, string contentType, string payload, string commitTag, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var content = payload == null
                        ? new StringContent(string.Empty)
                        : new StringContent(payload, Encoding.UTF8, contentType);

                if(!string.IsNullOrWhiteSpace(commitTag))
                    content.Headers.Add("X-REstate-CommitTag", new [] { commitTag });

                var response = await client.PostAsync(
                    $"instances/{instanceId}/fire/{triggerName}",
                    content).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            StateModel state = StringSerializer.Deserialize<StateModel>(responseBody);

            return state;
        }

        public async Task DeleteInstanceAsync(string machineInstanceId, CancellationToken cancellationToken)
        {
            await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.DeleteAsync($"{machineInstanceId}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return null; //TODO: make Task overload
            }, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetDiagramAsync(string instanceId, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"instances/{instanceId}/diagram").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            return responseBody;
        }

        public async Task<Machine> GetMachineAsync(string machineName, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineName}").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    throw GetException(response);
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            var configuration = responseBody != null ? StringSerializer.Deserialize<Machine>(responseBody) : null;

            return configuration;
        }

        public async Task<Machine> DefineStateMachineAsync(Machine configuration, CancellationToken cancellationToken)
        {
            var payload = StringSerializer.Serialize(configuration);

            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {

                var response = await client.PostAsync("machines/",
                    new StringContent(payload, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            var configurationResponse = StringSerializer.Deserialize<Machine>(responseBody);

            return configurationResponse;
        }

        public async Task<string> GetMachineDiagramAsync(string machineName, CancellationToken cancellationToken)
        {
            var responseBody = await EnsureAuthenticatedRequestAsync(async (client) =>
            {
                var response = await client.GetAsync($"machines/{machineName}/diagram").ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            return responseBody;
        }
    }
}
