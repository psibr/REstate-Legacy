using System;
using System.Configuration;
using System.Linq;
using Microsoft.Owin.Hosting;
using REstate.Owin;
using REstate.Client;

namespace SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = ConfigurationManager.AppSettings["REstate.url"];

            Startup.PassPhrase = ConfigurationManager.AppSettings["REstate.passphrase"];

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");

                var client = new REstateClient("http://localhost/restate/");
                using (var session = client.GetAuthenticatedSession("98EC17D7-7F31-4A44-A911-6B4D10B3DC2E").Result)
                {
                    var result = session.GetStateMachineConfiguration(1).Result;

                    var diagramForDefinition = session.GetMachineDiagram(1).Result;

                    var guid = session.InstantiateMachine(1).Result;

                    var diagramForInstance = session.GetMachineDiagram(guid).Result;

                    var triggers = session.GetAvailableTriggers(guid).Result;

                    var currentState = session.GetMachineState(guid).Result;

                    var isInState = session.IsMachineInState(guid, currentState.StateName).Result;

                    currentState = session.FireTrigger(guid, triggers.First().TriggerName).Result;

                    session.DeleteInstance(guid).Wait();

                    var newMachine = session.DefineStateMachine(result).Result;
                }

                Console.ReadLine();
            }
        }
    }
}