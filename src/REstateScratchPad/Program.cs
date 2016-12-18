using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using REstate.Configuration;

namespace REstateScratchPad
{
    public class Program
    {
        public static void Main(string[] args)
        {
/*            var restateClientFactory = new REstateClient.REstateClientFactory("http://localhost:5000/auth/apikey");
*/
/*            var configClient = restateClientFactory.GetConfigurationClient("http://localhost:5000");

            var session = configClient.GetSessionAsync("E17705B5-D0FD-47F5-849F-F0881B954C58", CancellationToken.None).Result;

            var act = new Action(() =>
            {

                var instanceId = session.InstantiateAsync("Load-Test-Machine3", CancellationToken.None).Result;

                var resultState = session.FireTriggerAsync(instanceId, "GoPathA", null, null, null, CancellationToken.None).Result;
            });*/

            //act.Invoke();

            var schema = JsonSchema4.FromType<InstanceRecord>();
            var schemaJson = schema.ToJson();

            File.WriteAllText("D:\\InstanceRecord.json", schemaJson);
            

        }
    }
}
