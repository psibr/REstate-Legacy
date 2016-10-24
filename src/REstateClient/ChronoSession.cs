using Newtonsoft.Json;
using REstateClient.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace REstateClient
{
    public class ChronoSession
        : AuthenticatedSession, IChronoSession
    {
        public ChronoSession(Uri apiKeyAuthAddress, Uri baseAddress, string apiKey, string token)
            : base(apiKeyAuthAddress, baseAddress, apiKey, token)
        {
        }

        public async Task AddChronoTrigger(string machineInstanceId, string chronoTriggerJson)
        {
            var chronoTrigger = JsonConvert.DeserializeObject<ChronoTriggerRequest>(chronoTriggerJson);

            chronoTrigger.MachineInstanceId = machineInstanceId;

            await AddChronoTrigger(chronoTrigger);
        }

        public async Task AddChronoTrigger(string machineInstanceId, string chronoTriggerJson, string payload)
        {
            var chronoTrigger = JsonConvert.DeserializeObject<ChronoTriggerRequest>(chronoTriggerJson);

            chronoTrigger.MachineInstanceId = machineInstanceId;

            chronoTrigger.Payload = payload;

            await AddChronoTrigger(chronoTrigger);
        }

        public async Task AddChronoTrigger(IChronoTriggerRequest chronoTrigger)
        {
            var payload = JsonConvert.SerializeObject(chronoTrigger);

            await EnsureAuthenticatedRequest(async (client) =>
            {
                var response = await client.PostAsync("triggers",
                    new StringContent(payload, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode) throw GetException(response);

                return await response.Content.ReadAsStringAsync();
            });
        }
    }
}
