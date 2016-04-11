using Newtonsoft.Json;
using REstate.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Client
{
    public class InstancesSession
        : AuthenticatedSession, IInstancesSession
    {
        public InstancesSession(Uri authBaseAddress, Uri baseAddress, string apiKey, string token)
            : base(authBaseAddress, baseAddress, apiKey, token)
        {

        }

        public async Task<string> InstantiateMachine(string machineDefinitionId)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.PostAsync($"instantiate/{machineDefinitionId}",
                    new StringContent(string.Empty));

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            var instance = JsonConvert.DeserializeObject<MachineInstantiateResponse>(responseBody);

            return instance.MachineInstanceId;
        }

        public async Task<State> GetMachineState(string machineInstanceId)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"{machineInstanceId}/state");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            });

            State state = JsonConvert.DeserializeObject<StateModel>(responseBody);

            return state;
        }

        public async Task<bool> IsMachineInState(string machineInstanceId, string stateName)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"{machineInstanceId}/isinstate/{stateName}");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            var isInStateResponse = JsonConvert.DeserializeObject<IsInStateResponse>(responseBody);

            return isInStateResponse.IsInState;
        }

        public async Task<ICollection<Trigger>> GetAvailableTriggers(string machineInstanceId)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"{machineInstanceId}/triggers");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            var triggers = JsonConvert.DeserializeObject<TriggerModel[]>(responseBody);

            return triggers.Select(t => (REstate.Trigger)t).ToArray();
        }

        public async Task<State> FireTrigger(string machineInstanceId, string triggerName, string payload = null)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.PostAsync($"{machineInstanceId}/fire/{triggerName}",
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

        public async Task DeleteInstance(string machineInstanceId)
        {
            await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.DeleteAsync($"{machineInstanceId}");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return null;
            });
        }

        public async Task<string> GetMachineDiagram(string machineInstanceId)
        {
            var responseBody = await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.GetAsync($"{machineInstanceId}/diagram");

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });

            return responseBody;
        }
    }
}