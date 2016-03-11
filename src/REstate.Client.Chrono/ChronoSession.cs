using Newtonsoft.Json;
using REstate.Client.Chrono.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Client.Chrono
{
    public class ChronoSession
        : AuthenticatedSession, IChronoSession
    {

        public ChronoSession(Uri apiKeyAuthAddress, Uri baseAddress, string apiKey, string token)
            : base(apiKeyAuthAddress, baseAddress, apiKey, token)
        {
        }


        public async Task AddChronoTrigger(Guid machineInstanceId, string chronoTriggerJson)
        {
            var chronoTrigger = JsonConvert.DeserializeObject<ChronoTriggerRequest>(chronoTriggerJson);

            chronoTrigger.MachineInstanceId = machineInstanceId;

            await AddChronoTrigger(chronoTrigger);

        }

        public async Task AddChronoTrigger(Guid machineInstanceId, string chronoTriggerJson, string payload)
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

                if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedException();

                return await response.Content.ReadAsStringAsync();
            });
        }
    }
}