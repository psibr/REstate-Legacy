using System;
using System.Configuration;
using System.Diagnostics;
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
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var result = session.GetStateMachineConfiguration(1).Result;

                    sw.Stop();

                    Console.WriteLine($"{sw.ElapsedMilliseconds}ms");

                    sw.Restart();
                    var newMachine = session.DefineStateMachine(result).Result;

                    sw.Stop();

                    Console.WriteLine($"{sw.ElapsedMilliseconds}ms");

                    sw.Restart();
                    var newMachine2 = session.DefineStateMachine(result).Result;

                    sw.Stop();

                    Console.WriteLine($"{sw.ElapsedMilliseconds}ms");
                }

                Console.ReadLine();
            }
        }
    }
}